using Autofac;
using TrainDatabase.Presentation.Files;
using TrainDatabase.Presentation.Infrastructure;

namespace TrainDatabase.UI;

/// <summary>
/// UI-head registrations: the Avalonia dispatcher (overriding the default) and the
/// Avalonia file picker. Registered after <c>PresentationModule</c> so these win.
/// </summary>
public class UiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AvaloniaUiDispatcher>().As<IUiDispatcher>().SingleInstance();
        builder.RegisterType<AvaloniaFilePicker>().As<IFilePicker>().SingleInstance();
    }
}
