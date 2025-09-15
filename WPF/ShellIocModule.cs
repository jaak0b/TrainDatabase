using Autofac;

namespace Shell.WPF
{
  public class ShellIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<MainWindow>()
             .AsSelf()
             .SingleInstance();
    }
  }
}