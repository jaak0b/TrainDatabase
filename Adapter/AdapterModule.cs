using Autofac;
using Core;
using Z21;

namespace Adapter
{
  public class AdapterModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<Client>().AsSelf().SingleInstance();
      builder.RegisterType<Z21ClientAdapter>().As<IClientAdapter>().SingleInstance();

      builder.RegisterType<ArduinoSpeedSensorAdapter>().As<ISpeedSensorPort>().InstancePerDependency();
      builder.Register<SpeedSensorPortFactory>(ctx =>
                                               {
                                                 IComponentContext c = ctx.Resolve<IComponentContext>();
                                                 return (portName, baudRate) => c.Resolve<ISpeedSensorPort>(new TypedParameter(typeof(string), portName),
                                                                                                            new TypedParameter(typeof(int), baudRate));
                                               })
             .SingleInstance();
    }
  }
}