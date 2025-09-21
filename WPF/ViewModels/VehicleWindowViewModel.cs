using Core.Presenters;
using Reactive.Bindings;
using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleWindowViewModel VehicleWindowViewModelFactory(int vehicleId);

  public class VehicleWindowViewModel
  {
    public VehicleWindowViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehicleManualControlViewFactory vehicleManualControlViewFactory,
                                  VehicleSettingsViewFactory vehicleSettingsViewFactory, IClientPresenter clientPresenter)
    {
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
      VehicleManualControlView = vehicleManualControlViewFactory(vehicleId);
      IsConnected = clientPresenter.IsConnected.ToReadOnlyReactiveProperty();
      VehicleSettingsView = vehicleSettingsViewFactory(vehicleId);
    }

    public ReadOnlyReactiveProperty<bool> IsConnected { get; }

    public VehicleViewModel VehicleViewModel { get; }

    public VehicleManualControlView VehicleManualControlView { get; }

    public VehicleSettingsView VehicleSettingsView { get; }
  }
}