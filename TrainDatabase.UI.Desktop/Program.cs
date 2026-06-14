using Avalonia;
using TrainDatabase.Composition;
using TrainDatabase.Presentation;
using TrainDatabase.UI;

namespace TrainDatabase.UI.Desktop;

internal static class Program
{
    // Avalonia requires an STA thread and must not use any Avalonia types before AppMain is called.
    [STAThread]
    public static void Main(string[] args)
    {
        // Build the composition root (registers Core + Infrastructure + Presentation + UI),
        // apply migrations, then hand the provider to the App.
        App.Services = Bootstrapper.InitializeAsync(
            loggerFactory: null,
            new PresentationModule(),
            new UiModule()).GetAwaiter().GetResult();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
