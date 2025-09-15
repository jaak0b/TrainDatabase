using Autofac;
using Z21;

namespace Core
{
  public class CoreIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<TrackPowerService>().AsSelf().SingleInstance();

      builder.RegisterType<VehicleService>().AsSelf().SingleInstance();

      builder.RegisterType<LogEventBus>().AsSelf().SingleInstance();

      builder.RegisterType<Client>().AsSelf().SingleInstance();
    }
  }
}