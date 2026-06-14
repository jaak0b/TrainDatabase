using System.Collections.Concurrent;
using Autofac;
using TrainDatabase.Core.Logging;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Services;

namespace TrainDatabase.Core;

/// <summary>
/// Registers the platform-agnostic domain services and presenters. Per-vehicle presenters
/// are memoised one-per-address (the cache pattern carried over from the WPF app).
/// </summary>
public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<VehicleControlService>().As<IVehicleControlService>().SingleInstance();
        builder.RegisterType<TrackService>().As<ITrackService>().SingleInstance();
        builder.RegisterType<VehicleSpeedCalibrationService>().As<IVehicleSpeedCalibrationService>().SingleInstance();
        builder.RegisterType<TrainPhysicsService>().AsSelf().SingleInstance();
        builder.RegisterType<LogEventBus>().AsSelf().SingleInstance();

        builder.RegisterType<ClientPresenter>().As<IClientPresenter>().SingleInstance();
        builder.RegisterType<TrackPresenter>().As<ITrackPresenter>().SingleInstance();

        builder.RegisterType<VehiclePresenter>().As<IVehiclePresenter>().InstancePerDependency();
        builder.Register<VehiclePresenterFactory>(context =>
        {
            ConcurrentDictionary<int, IVehiclePresenter> cache = new();
            IComponentContext c = context.Resolve<IComponentContext>();
            return vehicleId => cache.GetOrAdd(vehicleId,
                id => c.Resolve<IVehiclePresenter>(new TypedParameter(typeof(int), id)));
        }).SingleInstance();
    }
}
