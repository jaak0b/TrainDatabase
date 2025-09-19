using System;
using Core.Model;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;
using Z21.Events;

namespace Core.Presenters
{
  public interface IVehiclePresenter
  {
    Vehicle Vehicle { get; }

    ReactiveProperty<int> Speed { get; }

    ReactiveProperty<bool> Direction { get; }
  }

  public delegate IVehiclePresenter VehiclePresenterFactory(int trainAddress);

  public class VehiclePresenter : IVehiclePresenter
  {
    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, IClientAdapter client)
    {
      Vehicle = vehicleRepository.GetVehicleById(vehicleId) ?? throw new InvalidOperationException();

      client.VehicleData.Subscribe(VehicleData_OnNext);
    }

    private void VehicleData_OnNext(VehicleLiveData vehicleLiveData)
    {
      if(vehicleLiveData.VehicleAddress != Vehicle.Address)
        return;
      
      Speed.Value = vehicleLiveData.Speed;
      Direction.Value = vehicleLiveData.Direction;
    }

    public Vehicle Vehicle { get; }

    public ReactiveProperty<int> Speed { get; } = new();

    public ReactiveProperty<bool> Direction { get; } = new();
  }
}