using Autofac;
using Core;

namespace ClientAdapter
{
  public class ClientAdapterModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<Z21ClientAdapter>().As<IClientAdapter>().SingleInstance();
    }
  }
}