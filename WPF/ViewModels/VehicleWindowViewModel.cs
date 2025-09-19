using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleWindowViewModel VehicleWindowViewModelFactory(int vehicleId);

  public class VehicleWindowViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehicleManualControlViewFactory vehicleManualControlViewFactory)
  {

    public VehicleViewModel VehicleViewModel { get; } = vehicleViewModelFactory(vehicleId);

    public VehicleManualControlView VehicleManualControlView { get; } = vehicleManualControlViewFactory(vehicleId);
  }
}