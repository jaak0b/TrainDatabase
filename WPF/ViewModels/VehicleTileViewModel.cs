using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using Shell.WPF.Extensions;
using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleTileViewModel VehicleTileViewModelFactory(int vehicleId);

  public class VehicleTileViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehicleWindowFactory vehicleWindowFactory)
  {
    public VehicleViewModel VehicleViewModel { get; } = vehicleViewModelFactory(vehicleId);

    public ICommand CreateVehicleWindowCommand { get; } = new ActionCommand(o => vehicleWindowFactory(vehicleId).ShowOrActivate());
  }
}