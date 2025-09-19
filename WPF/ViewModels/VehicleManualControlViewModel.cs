using Core;
using Reactive.Bindings;
using Z21;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

  public class VehicleManualControlViewModel
  {
    private readonly IClientAdapter client;

    public VehicleManualControlViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, IClientAdapter client)
    {
      this.client = client;
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
    }

    public VehicleViewModel VehicleViewModel { get; }

    public ReactiveProperty<int> VehicleSpeed { get; set; } = new();
  }
}