using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using TrainDatabase.Composition;
using TrainDatabase.Presentation;
using TrainDatabase.UI;

namespace TrainDatabase.UI.Android;

/// <summary>
/// Android application entry: configures Avalonia and composes the app rooted at the
/// app-private files directory (Avalonia 12 hosts the app from the Application, not the Activity).
/// </summary>
[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        string filesDirectory = FilesDir?.AbsolutePath ?? System.IO.Path.GetTempPath();
        App.Services = Bootstrapper.InitializeAsync(
            loggerFactory: null,
            new PresentationModule(),
            new UiModule(),
            new AndroidModule(filesDirectory)).GetAwaiter().GetResult();

        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
}

[Activity(
    Label = "TrainDatabase",
    Theme = "@android:style/Theme.Material.Light.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
