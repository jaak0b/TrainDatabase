using Autofac;
using Autofac.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Browser;
using TrainDatabase.Core;
using TrainDatabase.Presentation;
using TrainDatabase.UI;

namespace TrainDatabase.UI.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        // Browser composition: shared Core + Presentation + UI with browser stubs
        // (no Infrastructure: WASM has no UDP/serial and persistence is deferred).
        ContainerBuilder builder = new();
        builder.RegisterModule(new CoreModule());
        builder.RegisterModule(new PresentationModule());
        builder.RegisterModule(new UiModule());
        builder.RegisterModule(new BrowserModule());

        App.Services = new AutofacServiceProvider(builder.Build());

        return BuildAvaloniaApp().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().WithInterFont();
}
