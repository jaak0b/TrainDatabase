using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Logging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TrainDatabase.Composition;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Presentation;
using TrainDatabase.Presentation.ViewModels;
using TrainDatabase.UI;
using TrainDatabase.UI.Views;

namespace TrainDatabase.UI.EndToEndTest;

/// <summary>
/// Renders the real Avalonia views (via the ViewLocator) headlessly and fails on any runtime
/// binding error. Compiled bindings catch most issues at build, but this covers DynamicResource
/// keys, value conversions (e.g. NumericUpDown long↔decimal), and ViewLocator resolution.
/// </summary>
public class RenderSmokeTests
{
    /// <summary>Collects Avalonia binding-area warnings/errors during a render.</summary>
    private sealed class CollectingLogSink : ILogSink
    {
        public List<string> BindingErrors { get; } = new();

        public bool IsEnabled(LogEventLevel level, string area) =>
            level >= LogEventLevel.Warning && area == LogArea.Binding;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        {
            if (IsEnabled(level, area)) BindingErrors.Add(messageTemplate);
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            if (IsEnabled(level, area)) BindingErrors.Add(messageTemplate);
        }
    }

    [AvaloniaTest]
    public void Shell_AndAllRoutes_RenderWithoutBindingErrors()
    {
        string baseDirectory = Path.Combine(Path.GetTempPath(), "TrainDatabase.Render", Guid.NewGuid().ToString("N"));
        FakeClientAdapter client = new();

        ILogSink? previousSink = Logger.Sink;
        CollectingLogSink sink = new();
        Logger.Sink = sink;

        IServiceProvider services = Bootstrapper.InitializeAsync(
            null, new PresentationModule(), new UiModule(), new TestOverrideModule(baseDirectory, client)).GetAwaiter().GetResult();

        try
        {
            App.Services = services;

            TrainDbContext context = services.GetRequiredService<TrainDbContext>();
            context.Vehicles.Add(new VehicleEntity { Name = "Render Loco", Address = 5 });
            context.SaveChanges();

            ShellViewModel shell = services.GetRequiredService<ShellViewModel>();
            Window window = new MainWindow { DataContext = shell };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            // Home route (vehicle panel) renders a tile.
            ((VehicleTilePanelViewModel)shell.Current!).Refresh();
            Dispatcher.UIThread.RunJobs();

            // Every top-level route must render (settings, import, management incl. the
            // ListBox item template with the parent-cast command bindings).
            shell.OpenSettingsCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            shell.OpenImportCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            shell.OpenManagementCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            shell.OpenMeasurementCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            // Detail route (manual control + settings forms incl. NumericUpDown).
            shell.GoHomeCommand.Execute(null);
            ((VehicleTilePanelViewModel)shell.Current!).Tiles[0].OpenCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            window.CaptureRenderedFrame();

            Assert.That(sink.BindingErrors, Is.Empty,
                $"Runtime binding errors:{Environment.NewLine}{string.Join(Environment.NewLine, sink.BindingErrors)}");
        }
        finally
        {
            Logger.Sink = previousSink;
            (services as IDisposable)?.Dispose();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (Directory.Exists(baseDirectory)) Directory.Delete(baseDirectory, true); }
            catch (IOException) { }
        }
    }
}
