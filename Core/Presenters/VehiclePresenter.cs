using System;
using System.Reactive.Linq;
using Core.Model;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Core.Presenters
{
  public interface IVehiclePresenter
  {
    ReactiveProperty<Vehicle> Vehicle { get; }

    ReactiveProperty<int> Speed { get; }

    ReactiveProperty<bool> Direction { get; }
  }

  public delegate IVehiclePresenter VehiclePresenterFactory(int trainAddress);

  public class VehiclePresenter : IVehiclePresenter
  {
    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, IClientAdapter client)
    {
      Vehicle.Value = vehicleRepository.GetVehicleById(vehicleId) ?? throw new InvalidOperationException();

      vehicleRepository.VehicleChangedStream
                       .Where(vehicle => vehicle.Id == vehicleId)
                       .ObserveOnUIDispatcher() // Maybe not needed? 
                       .Subscribe(updatedVehicle => Vehicle.Value = updatedVehicle);

      client.VehicleData.Subscribe(VehicleData_OnNext);
    }

    private void VehicleData_OnNext(VehicleLiveData vehicleLiveData)
    {
      if (vehicleLiveData.VehicleAddress != Vehicle.Value.Address)
        return;

      Speed.Value = vehicleLiveData.Speed;
      Direction.Value = vehicleLiveData.Direction;
    }

    public ReactiveProperty<Vehicle> Vehicle { get; } = new();

    public ReactiveProperty<int> Speed { get; } = new();

    public ReactiveProperty<bool> Direction { get; } = new();
  }
}