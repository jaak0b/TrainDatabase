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
      builder.RegisterType<VehicleTilePanelViewModel>().AsSelf().SingleInstance();
      builder.RegisterType<VehicleTilePanelView>().AsSelf().SingleInstance();

      builder.RegisterType<DatabaseImportView>().AsSelf().SingleInstance();

      builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
      builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

      builder.RegisterType<VehicleViewModel>().AsSelf().InstancePerDependency();
      builder.Register<VehicleViewModelFactory>(ctx =>
                                                {
                                                  ConcurrentDictionary<int, VehicleViewModel> cache = new();
                                                  IComponentContext c = ctx.Resolve<IComponentContext>();
                                                  return address => cache.GetOrAdd(address, i => c.Resolve<VehicleViewModel>(new TypedParameter(typeof(int), i)));
                                                })
             .SingleInstance();

      builder.RegisterType<VehicleTileViewModel>().AsSelf().InstancePerDependency();
      builder.Register<VehicleTileViewModelFactory>(ctx =>
                                                    {
                                                      ConcurrentDictionary<int, VehicleTileViewModel> cache = new();
                                                      IComponentContext c = ctx.Resolve<IComponentContext>();
                                                      return address => cache.GetOrAdd(address, i => c.Resolve<VehicleTileViewModel>(new TypedParameter(typeof(int), i)));
                                                    })
             .SingleInstance();

      builder.RegisterType<VehicleWindow>().AsSelf().InstancePerDependency();
      builder.Register<VehicleWindowFactory>(ctx =>
                                             {
                                               ConcurrentDictionary<int, VehicleWindow> cache = new();
                                               IComponentContext c = ctx.Resolve<IComponentContext>();
                                               return address => cache.GetOrAdd(address,
                                                                                i =>
                                                                                {
                                                                                  VehicleWindow window = c.Resolve<VehicleWindow>(new TypedParameter(typeof(int), i));
                                                                                  window.Closed += (sender, args) => cache.TryRemove(i, out _);
                                                                                  return window;
                                                                                });
                                             })
             .SingleInstance();

      builder.RegisterType<VehicleManualControlView>().AsSelf().InstancePerDependency();
      builder.Register<VehicleManualControlViewFactory>(ctx =>
                                                        {
                                                          ConcurrentDictionary<int, VehicleManualControlView> cache = new();
                                                          IComponentContext c = ctx.Resolve<IComponentContext>();
                                                          return address => cache.GetOrAdd(address, i => c.Resolve<VehicleManualControlView>(new TypedParameter(typeof(int), i)));
                                                        })
             .SingleInstance();

      builder.RegisterType<VehicleManualControlViewModel>().AsSelf().InstancePerDependency();
      builder.Register<VehicleManualControlViewModelFactory>(ctx =>
                                                             {
                                                               ConcurrentDictionary<int, VehicleManualControlViewModel> cache = new();
                                                               IComponentContext c = ctx.Resolve<IComponentContext>();
                                                               return address => cache.GetOrAdd(address, i => c.Resolve<VehicleManualControlViewModel>(new TypedParameter(typeof(int), i)));
                                                             })
             .SingleInstance();
      
      builder.RegisterType<VehicleWindowViewModel>().AsSelf().InstancePerDependency();
      builder.Register<VehicleWindowViewModelFactory>(ctx =>
                                                      {
                                                        ConcurrentDictionary<int, VehicleWindowViewModel> cache = new();
                                                        IComponentContext c = ctx.Resolve<IComponentContext>();
                                                        return address => cache.GetOrAdd(address, i => c.Resolve<VehicleWindowViewModel>(new TypedParameter(typeof(int), i)));
                                                      })
             .SingleInstance();
    }
  }
}