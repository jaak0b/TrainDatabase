using Autofac;
using Core;
using Z21;

namespace ClientAdapter
{
  public class ClientAdapterModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<Client>().AsSelf().SingleInstance();
      builder.RegisterType<Z21ClientAdapter>().As<IClientAdapter>().SingleInstance();
    }
  }
}