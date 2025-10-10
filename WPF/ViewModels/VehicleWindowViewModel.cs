using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleWindowViewModel VehicleWindowViewModelFactory(int vehicleId);

  public class VehicleWindowViewModel
  {
    public VehicleWindowViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehicleManualControlViewFactory vehicleManualControlViewFactory,
                                  VehicleSettingsViewFactory vehicleSettingsViewFactory)
    {
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
      VehicleManualControlView = vehicleManualControlViewFactory(vehicleId);
      VehicleSettingsView = vehicleSettingsViewFactory(vehicleId);
    }

    public VehicleViewModel VehicleViewModel { get; }

    public VehicleManualControlView VehicleManualControlView { get; }

    public VehicleSettingsView VehicleSettingsView { get; }
  }
}