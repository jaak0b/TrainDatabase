using System.Collections.Concurrent;
using Autofac;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation;

/// <summary>
/// Registers view models, navigation and dialog services. Per-vehicle view models are
/// memoised one-per-address via factory delegates, mirroring the WPF cache pattern.
/// The UI head replaces <see cref="ImmediateUiDispatcher"/> with an Avalonia dispatcher.
/// </summary>
public class PresentationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ImmediateUiDispatcher>().As<IUiDispatcher>().SingleInstance();

        builder.RegisterType<NavigationService>().AsSelf().As<INavigationService>().SingleInstance();
        builder.RegisterType<DialogService>().AsSelf().As<IDialogService>().SingleInstance();

        builder.RegisterType<ShellViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<VehicleTilePanelViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<DatabaseImportViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<VehicleManagementViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<MeasurementViewModel>().AsSelf().SingleInstance();

        builder.RegisterType<VehicleTileViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<VehicleDetailViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<VehicleManualControlViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<VehicleSettingsViewModel>().AsSelf().InstancePerDependency();

        RegisterFactory<VehicleTileViewModel, VehicleTileViewModelFactory>(builder);
        RegisterFactory<VehicleDetailViewModel, VehicleDetailViewModelFactory>(builder);
        RegisterFactory<VehicleManualControlViewModel, VehicleManualControlViewModelFactory>(builder);
        RegisterFactory<VehicleSettingsViewModel, VehicleSettingsViewModelFactory>(builder);
    }

    /// <summary>
    /// Registers a memoising <typeparamref name="TFactory"/> that resolves one cached
    /// <typeparamref name="TViewModel"/> per vehicle id.
    /// </summary>
    private static void RegisterFactory<TViewModel, TFactory>(ContainerBuilder builder)
        where TViewModel : notnull
        where TFactory : Delegate
    {
        builder.Register(context =>
        {
            ConcurrentDictionary<int, TViewModel> cache = new();
            IComponentContext c = context.Resolve<IComponentContext>();
            Func<int, TViewModel> resolve = id => cache.GetOrAdd(id,
                key => c.Resolve<TViewModel>(new TypedParameter(typeof(int), key)));
            return (TFactory)Delegate.CreateDelegate(typeof(TFactory), resolve.Target, resolve.Method);
        }).As<TFactory>().SingleInstance();
    }
}
