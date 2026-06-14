# CLAUDE.md

TrainDatabase is a cross-platform (Avalonia 12: Desktop / Android / Browser) app for controlling model trains through a Roco/Fleischmann **Z21** digital command station, with optional Arduino-based speed measurement for reliable double-traction. The codebase is mid-rewrite from the old WPF solution to a layered Avalonia one; some of the rules below describe the **target** state and call out where current code hasn't caught up.

## Hard Rules

1. **Prefer instance methods over static.** Static is reserved for: Avalonia `AvaloniaProperty.Register` and framework metadata, the platform `Program` entry points, and the Composition `Bootstrapper`. Don't introduce new static methods/properties for domain logic — they can't be faked and break DI.
2. **Localization is a goal, not yet implemented.** There is no resx/localization layer today (UI strings are hardcoded, partly German). When localization is added it must be **resx-only**: every translatable string in `Strings.en/de.resx` (or a domain-specific resx pair), referenced through a `LocalizationService`, with both language files holding every key. Until then, do not scatter a new ad-hoc string mechanism — flag the need and follow this approach when the layer lands.
3. **TDD mandatory, test-first, no exceptions.** For EVERY behavior change incl. bug fixes: commit the test before the production code. Order is non-negotiable: (a) write the test, (b) run it and PASTE the failing output, (c) only then touch production code, (d) re-run to green. A red run you can quote is the gate — no red proof = the fix does not start. Writing the fix first, or "I'll add a test after", is a rule violation; if you catch yourself having edited production code first, revert it and restart from (a).
4. **Three test layers per change.** Every feature and bug fix needs unit (Core/Presentation) + integration (Infrastructure) + headless (UI E2E) tests. "It's only a small change" is not an exemption — if it changes behavior, all three layers apply. Untestable-by-design code (pure XAML, generated code) is the only exception, and you must say so explicitly.
5. **Verification gate — scaled to the change.** A change is NOT done until it has been verified and the real command output quoted. Never claim a feature is finished, never hand back to the user, and never commit on *assumed* results. The depth of the gate depends on the blast radius of the change:

   - **Small, localized change** (a few files inside one project, no cross-project/API/schema/DI surface touched — e.g. a single ViewModel tweak, one converter, a presenter detail): run only the **directly relevant test fixtures** (`dotnet test … --filter`) and quote their totals. Coverage and mutation are **not** required. State that you classified it as small and which fixtures you ran.
   - **Big or multi-file / multi-project change** (touches more than one project, or changes a public API/port, DB schema/migration, DI wiring, the Z21/Arduino adapters, the Z21 import format, or shared infrastructure): run the **full gate** below, all steps, output quoted.

   When unsure which bucket applies, **ask the user for confirmation** before choosing — do not silently pick one. The TDD red-proof (rule #3) and three-layer thinking (rule #4) apply to *every* change regardless of size; this rule scales only *how much of the suite* you run to verify.

   Full gate:
   1. **Full suite green.** Run the complete `.\build.ps1 --target Test` (not just the fixtures you touched) and paste the pass/fail totals. A single failure blocks everything.
   2. **Coverage ≥95% and not dropped.** Run `.\build.ps1 --target Coverage` and quote the exact merged line-coverage number it prints. **Reality check:** the in-migration baseline is currently well below target (~71%). Until the suite catches up, the 95% gate will fail by design — either add tests to climb toward it, or run `.\build.ps1 --target Coverage --coverage-threshold <baseline>` with the measured baseline and state it explicitly. Whatever you do, **never let coverage drop versus where it was** — a regression means add tests until it recovers or prove why with the measured baseline.
   3. **Mutation testing — scoped to your local changes only, surviving mutants addressed. Running full Stryker is forbidden.** Stryker over the whole solution takes far too long; never do it. Always run it scoped to your diff: `.\build.ps1 --target Mutate` `git diff`s against `HEAD` and passes only those changed `.cs` files to Stryker, so it covers just the code you have changed since your last commit (your uncommitted working-tree changes). Run it **before you commit** — once your work is committed there is nothing left in the diff to mutate. Override the baseline only when you need a wider sweep (`.\build.ps1 --target Mutate --since <branch-or-commit>`). Stop the running Desktop app first (`Get-Process TrainDatabase.UI.Desktop | Stop-Process -Force`) — a live instance locks `TrainDatabase.UI.dll` and fails Stryker's build. Quote the mutation score and review survivors in the code you changed; kill them with tests or justify each explicitly.
   4. **Manual UI verification (for UI changes).** Ask the user to run the app with exact repro steps (see "Verifying UI Fixes"). Tests do not replace this; they are in addition to it.

   If any gate cannot be completed (e.g. a pre-existing failure you did not introduce), STOP and surface it to the user with the evidence — do not quietly proceed as if it passed.
6. **No test touches the developer's DB or filesystem.** Use a throwaway SQLite database under a unique `Path.GetTempPath()` folder (the `TempDatabase` helper, which runs the real EF migrations) and `Path.GetTempPath()` temp dirs, all deleted in teardown. Never let a test reach `%APPDATA%\TrainDatabase`; storage paths are injectable (`DesktopAppStorage(baseDirectory)`) precisely so tests stay isolated.
7. **No empty catch blocks.** Log via the injected `ILogger`/`ILoggerFactory` (and the in-app `LogEventBus` where the message should surface in the UI), and for user-initiated operations surface failures through `IDialogService`.
8. **No trademarked words in files.**
9. **NuGet packages: official Microsoft or highly-regarded community only.** No niche/unmaintained single-author packages. Prefer built-in BCL APIs over third-party dependencies. Current sanctioned stack: Autofac, EF Core (+ Sqlite), Mapster, System.Reactive, CommunityToolkit.Mvvm, Avalonia, the `Z21` client package, Dapper, System.IO.Ports.
10. **No code comments.** Code self-explains via names and structure, in all we author (C#, XAML, YAML, JSON, `.csproj`). Banned: *what*-narration (`// build the menu`), divider banners, commented-out code (git is the history). The XML doc-comments currently scattered through the rewrite are **legacy** — don't add new ones; phase them out by renaming until they're redundant. Only allowed: a short non-obvious **why** the code can't express (external-bug workaround, Avalonia gotcha, the Z21 protocol quirk). Markdown docs are exempt.
11. **Commits require owner review first.** Never run `git commit` or `git push` until the repository owner has explicitly approved the change in chat. Present the diff summary and ask; only proceed after a clear "yes" (or equivalent). Once approved: no AI attribution in anything that touches git or GitHub — not in commit messages, PR titles or descriptions, issue/PR comments, tags, or release notes. No `Co-Authored-By` trailer, no "Generated with" line, no AI author/committer identity. This applies to every git and `gh`/GitHub API action without exception. Commits carry the human's authorship only. **Commit messages are a short, single sentence** — one line, no body, no bullet list; if a change feels too big to describe in one sentence, split it into smaller commits.
12. **No positional tuple access — code must be refactor-safe.** Never read a tuple by element position (`.Item1`/`.Item2`) and never destructure one positionally (`var (a, b) = …`). Every multi-value return is a named `record` / `record struct` whose members are read by name, so reordering or renaming a member is a compile error, not a silent value swap. This applies to return types, locals, and method results alike; a private named-element `ValueTuple` is tolerated only when it is never destructured positionally — when in doubt, declare a record.
13. **Every new feature is documented.** Update the README (and any future `docs-src/**` pages) in the same change. Write in a human, conversational style — not terse machine-speak.
14. **Self-review-and-fix before handoff — multi-file changes only.** Before presenting a change that touches more than one file for commit approval, run a medium-effort `/code-review` scoped to the change and **fix every finding it surfaces** — each fix following the TDD (rule #3) and verification (rule #5) rules — before the change is done. The review's own verify step already discards false positives, so any finding that reaches the list is real: it must be fixed, never merely listed. The change is NOT done while a single surfaced finding remains open. Single-file changes are exempt, mirroring the verification-gate buckets in rule #5. This is part of the Definition of Done — the owner should never have to ask for it.

## Definition of Done — run this checklist before calling any change "finished"

A feature or fix is complete **only** when every box below is genuinely ticked, with real command output quoted (not assumed, not "should pass"). If you cannot tick a box, the work is not done — say so and stop.

- [ ] **Tests written first** (rule #3) — red output quoted before the production code existed.
- [ ] **All three layers present** (rule #4) — unit + integration + headless, or an explicit note on why a layer doesn't apply.
- [ ] **Tests run, scaled to the change** (rule #5) — small localized change: relevant fixtures green, totals quoted, classification stated. Big/multi-project change: full suite green (`.\build.ps1 --target Test`), totals quoted.
- [ ] **Coverage ≥95% and not dropped** (rule #5.2) — *big changes only*; exact number quoted; regressions explained with a measured baseline (note the current ~71% migration baseline).
- [ ] **Mutation run scoped to local changes, survivors handled** (rule #5.3) — *big changes only*; run `.\build.ps1 --target Mutate` (diff vs `HEAD`, your uncommitted changes) **before** committing — never full Stryker; Desktop app stopped first; score quoted; new survivors killed or justified.
- [ ] **Manual UI verification requested** (rule #5.4) — for any UI change, exact repro steps handed to the user.
- [ ] **Docs updated** (rule #13).
- [ ] **Localization** (rule #2) — if/when a resx layer exists, every new key in both `Strings.en.resx` and `Strings.de.resx`; otherwise note that no localization layer exists yet.
- [ ] **No code comments added** (rule #10) — re-read the diff; the only comments left are genuine non-obvious *why* notes, never *what*-narration, new XML doc-comments, or commented-out code.
- [ ] **Self-review run and every finding fixed** (rule #14) — multi-file change: medium `/code-review` on the diff; every surfaced finding fixed before done — none merely listed. Single-file change: state the exemption.
- [ ] **Owner review obtained** (rule #11) — diff summary presented in chat and owner has explicitly approved before any `git commit` or `git push`.

Do not compress this gate to save time. "Looks done" is not done; the checklist is what makes it done.

## Build & Run

```powershell
try { Get-Process -Name "TrainDatabase.UI.Desktop" | Stop-Process -Force } catch {}
dotnet build "TrainDatabase.UI.Desktop\TrainDatabase.UI.Desktop.csproj"
.\TrainDatabase.UI.Desktop\bin\Debug\net8.0\TrainDatabase.UI.Desktop.exe

.\build.ps1 --target Test                          # build + run all tests (default target)
.\build.ps1 --target Coverage                      # tests + merged coverage report, enforces the threshold
.\build.ps1 --target Coverage --coverage-threshold 70   # override the gate while coverage climbs
.\build.ps1 --target Mutate                        # mutation testing — scoped to your uncommitted changes since HEAD (full runs forbidden)
.\build.ps1 --target Mutate --since main           # wider mutation sweep vs another branch/commit
dotnet test "TrainDatabase.Core.UnitTest\TrainDatabase.Core.UnitTest.csproj" --filter "FullyQualifiedName~MethodName"
dotnet ef migrations add <Name> --project TrainDatabase.Infrastructure
```

The build is **Nuke** (`build.ps1` / `build.sh` bootstrap → `_build/` project). `build.ps1` runs `dotnet tool restore` first, so Stryker (`dotnet-stryker`, pinned in `.config/dotnet-tools.json`) is available. Coverage uses coverlet's XPlat collector merged by ReportGenerator into `artifacts/coverage/` (gitignored).

> **Data/log location** (`DesktopAppStorage`): everything lives under a per-user base directory, resolved as **explicit arg > `TRAINDATABASE_DATA_DIR` env var > `%APPDATA%\TrainDatabase`**:
> - `…\Data\Database.sqlite` — the SQLite database
> - `…\Data\VehicleImage\` — vehicle images
> - `…\Log\` — log files
>
> Set `TRAINDATABASE_DATA_DIR` to an isolated folder for throwaway/dev runs so you never disturb real data. **Migrations run on startup**: each platform head calls `Bootstrapper.InitializeAsync`, which resolves `IDatabaseInitializer` and applies EF migrations (with a baseline-stamping cut-over for pre-rewrite databases). A startup crash → read the log under the resolved `…\Log\` directory.

## Project Structure

| Project | Role |
|---|---|
| `TrainDatabase.Core` | Domain models, ports (`IClientAdapter`, `ISpeedSensorPort`, repository/storage interfaces), services, presenters, reactive primitives (`ObservableValue`), logging abstractions |
| `TrainDatabase.Infrastructure` | EF Core SQLite (`TrainDbContext`, migrations, `DatabaseInitializer`), Mapster mapping, Z21 client + Arduino serial adapters (`Hardware/`), Z21 layout import (`Import/`), platform storage (`Platform/`) |
| `TrainDatabase.Presentation` | ViewModels (CommunityToolkit.Mvvm), `INavigationService`, `IDialogService`, file pickers — platform-agnostic UI logic |
| `TrainDatabase.UI` | Avalonia views, `App`, `ViewLocator`, converters, `UiModule` |
| `TrainDatabase.UI.Desktop` / `.Android` / `.Browser` | Platform heads (entry points + platform services) |
| `TrainDatabase.Composition` | `Bootstrapper` — builds the Autofac container from the Core/Infrastructure modules and applies migrations |
| `TrainDatabase.SpeedSensorConsole` | Standalone console for the Arduino speed sensor |
| `*.UnitTest` / `*.IntegrationTest` / `*.EndToEndTest` | Unit (Core, Presentation), Integration (Infrastructure), Headless E2E (UI) |

## Key Patterns

**DI:** Autofac modules — `CoreModule`, `InfrastructureModule`, `PresentationModule`, `UiModule`. `Composition.Bootstrapper.InitializeAsync(loggerFactory, params Module[])` populates an `IServiceProvider`, registers Infrastructure + Core, then any extra (platform) modules, and runs `IDatabaseInitializer`. Each platform `Program` calls it with its own `PresentationModule` + `UiModule`.

**Navigation:** service-based — `INavigationService` / `NavigationService` drive which ViewModel is shown; `ShellViewModel` is the root. `ViewLocator` maps `XxxViewModel → XxxView` by convention.

**ViewModels:** CommunityToolkit.Mvvm — `ViewModelBase : ObservableObject`. `[RelayCommand]`-generated commands follow the `DoThingAsync` → `DoThingCommand` convention; `[ObservableProperty]` generates the public property from a backing field.

**Reactive state:** Core exposes live values as `IObservableValue<T>` / `ObservableValue<T>` and `IObservable<T>` streams (System.Reactive) — adapters (Z21, Arduino) push into subjects, presenters/ViewModels subscribe.

**Mapping:** Mapster between EF entities and domain models (see `Infrastructure/Mapping/`).

## Avalonia 12 Gotchas

- **Dynamic `MenuItem` submenus:** build in code-behind (`CollectionChanged` → hand-built `List<MenuItem>`). XAML `ItemsSource` binding does not render submenus in Avalonia 12.
- **`Button.Flyout` content declared in XAML never receives input:** the popup renders and its bindings resolve (headless tests pass!), but real clicks die with `(PresentationSource) PlatformImpl is null, couldn't handle input` in the log. Build flyout content in code-behind (`new Flyout()` + content controls).
- **`IsVisible` on a null sub-path** evaluates `true` when the object is null — always add `FallbackValue=False`.
- **Never replace an `ObservableCollection` instance** — mutate in place (`Clear()` + `Add()`). Flyout menus re-bind unreliably to a replaced collection.
- **Compiled bindings:** `AvaloniaUseCompiledBindingsByDefault=true`. All `DataTemplate`s need `x:DataType`.

## Testing

Framework is **NUnit** with the **coverlet** collector. Mocking is **hand-rolled fakes** (no FakeItEasy/Moq) — see each test project's `Fakes/` folder (e.g. `FakeClientAdapter` implementing `IClientAdapter`, `FakeVehiclePresenter` implementing `IVehiclePresenter`).

**Conventions:** fixture = `<ClassUnderTest>Test(s)`, method = `MethodName_State_Expected`. One fixture per production class, one file per fixture. Never a catch-all fixture name.

**Core / Presentation tests:** plain NUnit + hand-rolled fakes for the port/presenter interfaces.

**Infrastructure tests:** throwaway SQLite under a unique `Path.GetTempPath()` folder via the `TempDatabase` helper (runs the real EF migrations), deleted on dispose — never touch the real database.

**UI E2E tests:** `Avalonia.Headless.NUnit` (`[AvaloniaTest]`); shared setup in `TestInfrastructure`. These render real views against the composition root.

## Verifying UI Fixes

Ask the user to run the app with exact repro steps. Do not automate. Tests are required *in addition* to manual verification, never instead.
