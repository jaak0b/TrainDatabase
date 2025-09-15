using Autofac;
using Shell.WPF.DatabaseImport;

namespace Shell.WPF
{
  public class ShellIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<DatabaseImportView>().AsSelf().SingleInstance();
      
      builder.RegisterType<MainWindow>()
             .AsSelf()
             .SingleInstance();
    }
  }
}