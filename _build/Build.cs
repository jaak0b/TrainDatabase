using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Minimum merged line-coverage percentage the Coverage target enforces.")]
    readonly double CoverageThreshold = 95;

    [Parameter("Git base the Mutate target diffs against to pick files to mutate - Default is 'HEAD'.")]
    readonly string Since = "HEAD";

    AbsolutePath SolutionFile => RootDirectory / "TrainDatabase.slnx";
    AbsolutePath StrykerConfig => RootDirectory / "stryker-config.json";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath CoverageReportDirectory => ArtifactsDirectory / "coverage";

    Target Clean => _ => _
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(SolutionFile));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .EnableNoBuild());
        });

    Target Coverage => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();

            DotNetTest(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetDataCollector("XPlat Code Coverage")
                .SetResultsDirectory(TestResultsDirectory));

            ReportGenerator(s => s
                .SetReports(TestResultsDirectory / "**" / "coverage.cobertura.xml")
                .SetTargetDirectory(CoverageReportDirectory)
                .SetReportTypes(ReportTypes.Html, ReportTypes.TextSummary, ReportTypes.Cobertura));

            double lineCoverage = ReadLineCoverage(CoverageReportDirectory / "Summary.txt");
            Log.Information("Merged line coverage: {Coverage}%", lineCoverage);
            Assert.True(lineCoverage >= CoverageThreshold,
                $"Line coverage {lineCoverage}% is below the required {CoverageThreshold}%.");
        });

    Target Mutate => _ => _
        .Executes(() =>
        {
            string[] changedFiles = Git($"diff --name-only {Since} -- \"*.cs\"")
                .Select(x => x.Text.Trim())
                .Where(x => x.Length > 0)
                .Where(x => !x.Contains("/obj/") && !x.Contains("/bin/") && !x.StartsWith("_build/"))
                .ToArray();

            if (changedFiles.Length == 0)
            {
                Log.Warning("No changed C# files versus {Since}; nothing to mutate.", Since);
                return;
            }

            Log.Information("Mutating {Count} changed file(s) versus {Since}:", changedFiles.Length, Since);
            changedFiles.ForEach(x => Log.Information("  {File}", x));

            string mutateArgs = changedFiles.Select(x => $"--mutate \"{x}\"").JoinSpace();
            DotNet($"stryker --solution \"{SolutionFile}\" --config-file \"{StrykerConfig}\" {mutateArgs}",
                workingDirectory: RootDirectory);
        });

    static double ReadLineCoverage(AbsolutePath summaryFile)
    {
        Assert.FileExists(summaryFile);
        string line = summaryFile.ReadAllLines()
            .FirstOrDefault(x => x.Contains("Line coverage:", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception($"No 'Line coverage:' entry found in {summaryFile}.");
        string value = line.Split(':').Last().Replace("%", string.Empty).Trim();
        return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
