using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TrainDatabase.Presentation.ViewModels;
using TrainDatabase.UI.Views;

namespace TrainDatabase.UI;

public partial class App : Application
{
    /// <summary>Set by the platform head's composition root before the app starts.</summary>
    public static IServiceProvider? Services { get; set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && Services is not null)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<ShellViewModel>(),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView && Services is not null)
        {
            singleView.MainView = new ShellView
            {
                DataContext = Services.GetRequiredService<ShellViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
