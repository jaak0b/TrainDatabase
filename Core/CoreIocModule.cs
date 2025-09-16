using System.Collections.Concurrent;
using Autofac;
using Core.ConfigurationImport;
using Core.ConfigurationImport.Z21New;
using Core.Factories;
using Core.Presenters;
using Z21;

namespace Core
{
  public class CoreIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<Z21NewDatabaseImporter>().As<IDatabaseImporter>().SingleInstance();

      builder.RegisterType<TrackPowerService>().AsSelf().SingleInstance();

      builder.RegisterType<VehicleService>().AsSelf().SingleInstance();

      builder.RegisterType<LogEventBus>().AsSelf().SingleInstance();

      builder.RegisterType<Client>().AsSelf().SingleInstance();

      builder.RegisterType<VehiclePresenterOld>().AsSelf().InstancePerDependency();

      builder.RegisterType<VehiclePresenter>().AsSelf().InstancePerDependency();
      builder.Register<VehiclePresenterFactory>(ctx =>
                                                {
                                                  ConcurrentDictionary<ushort, VehiclePresenter> cache = new();
                                                  IComponentContext c = ctx.Resolve<IComponentContext>();
                                                  return address => cache.GetOrAdd(address, i => c.Resolve<VehiclePresenter>(new TypedParameter(typeof(ushort), i)));
                                                })
             .SingleInstance();
    }
  }
}