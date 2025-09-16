using System.Collections.Concurrent;
using Autofac;
using Shell.WPF.DatabaseImport;
using Shell.WPF.ViewModels;
using Shell.WPF.Views;

namespace Shell.WPF
{
  public class ShellIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<DatabaseImportView>().AsSelf().SingleInstance();
      builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

      builder.RegisterType<VehicleViewModel>().AsSelf().InstancePerDependency();
      builder.Register<VehicleViewModelFactory>(ctx =>
                                                {
                                                  ConcurrentDictionary<ushort, VehicleViewModel> cache = new();
                                                  IComponentContext c = ctx.Resolve<IComponentContext>();
                                                  return address => cache.GetOrAdd(address, i => c.Resolve<VehicleViewModel>(new TypedParameter(typeof(ushort), i)));
                                                })
             .SingleInstance();

      builder.RegisterType<VehicleTileView>().AsSelf().InstancePerDependency();
      builder.Register<VehicleTileViewFactory>(ctx =>
                                               {
                                                 ConcurrentDictionary<ushort, VehicleTileView> cache = new();
                                                 IComponentContext c = ctx.Resolve<IComponentContext>();
                                                 return address => cache.GetOrAdd(address, i => c.Resolve<VehicleTileView>(new TypedParameter(typeof(ushort), i)));
                                               })
             .SingleInstance();
    }
  }
}