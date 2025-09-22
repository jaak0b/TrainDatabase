using System.Collections.Concurrent;
using Autofac;
using Core.ConfigurationImport;
using Core.ConfigurationImport.Z21New;
using Core.Presenters;
using Core.Services;
using Z21;

namespace Core
{
  public class CoreIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<VehicleSpeedCalibrationService>().As<IVehicleSpeedCalibrationService>().SingleInstance();

      builder.RegisterType<TrackService>().As<ITrackService>().SingleInstance();

      builder.RegisterType<TrackPresenter>().As<ITrackPresenter>().SingleInstance();

      builder.RegisterType<VehicleControlService>().As<IVehicleControlService>().SingleInstance();

      builder.RegisterType<ClientPresenter>().As<IClientPresenter>().SingleInstance();

      builder.RegisterType<Z21NewDatabaseImporter>().As<IDatabaseImporter>().SingleInstance();

      builder.RegisterType<TrackPowerService>().AsSelf().SingleInstance();

      builder.RegisterType<VehicleService>().AsSelf().SingleInstance();

      builder.RegisterType<LogEventBus>().AsSelf().SingleInstance();

      builder.RegisterType<VehiclePresenterOld>().AsSelf().InstancePerDependency();

      builder.RegisterType<VehiclePresenter>().As<IVehiclePresenter>().InstancePerDependency();
      builder.Register<VehiclePresenterFactory>(ctx =>
                                                {
                                                  ConcurrentDictionary<int, IVehiclePresenter> cache = new();
                                                  IComponentContext c = ctx.Resolve<IComponentContext>();
                                                  return address => cache.GetOrAdd(address, i => c.Resolve<IVehiclePresenter>(new TypedParameter(typeof(int), i)));
                                                })
             .SingleInstance();
    }
  }
}