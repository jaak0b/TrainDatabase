namespace Shell.WPF.ViewModels
{
  public delegate VehicleSettingsViewModel VehicleSettingsViewModelFactory(int vehicleId);

  public class VehicleSettingsViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory)
  {

    public VehicleViewModel VehicleViewModel { get; } = vehicleViewModelFactory(vehicleId);
  }
}