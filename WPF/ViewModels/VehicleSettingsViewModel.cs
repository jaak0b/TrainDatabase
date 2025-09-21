using System.Threading.Tasks;
using Core.Presenters;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleSettingsViewModel VehicleSettingsViewModelFactory(int vehicleId);

  public class VehicleSettingsViewModel
  {
    private readonly IVehicleRepository vehicleRepository;

    public VehicleSettingsViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehiclePresenterFactory vehiclePresenterFactory, IVehicleRepository vehicleRepository)
    {
      this.vehicleRepository = vehicleRepository;
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
      VehiclePresenter = vehiclePresenterFactory(vehicleId);

      Vehicle = VehiclePresenter.Vehicle.ToReactiveProperty()!;
    }

    public ReactiveProperty<Vehicle> Vehicle { get; set; }

    public VehicleViewModel VehicleViewModel { get; }

    public IVehiclePresenter VehiclePresenter { get; }

    public async Task SaveChangesAsync()
    {
      await vehicleRepository.UpdateVehicleAsync(Vehicle.Value);
    }

    public void RevertChanges()
    {
      vehicleRepository.RevertVehicleChange(Vehicle.Value.Id);
    }
  }
}