namespace Shell.WPF.ViewModels
{
  public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

  public class VehicleManualControlViewModel
  {

    public VehicleManualControlViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory)
    {
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
    }

    public VehicleViewModel VehicleViewModel { get; }
  }
}