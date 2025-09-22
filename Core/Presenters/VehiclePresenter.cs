using System;
using System.Reactive.Linq;
using Core.Model;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;

namespace Core.Presenters
{
  public interface IVehiclePresenter
  {
    ReactiveProperty<Vehicle> Vehicle { get; }

    ReactiveProperty<int> MaximumSpeedStep { get; }

    ReactiveProperty<int> Speed { get; }

    ReactiveProperty<bool> Direction { get; }
  }

  public delegate IVehiclePresenter VehiclePresenterFactory(int trainAddress);

  public class VehiclePresenter : IVehiclePresenter
  {
    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, IClientAdapter client)
    {
      UpdatePropertiesOnSubscribe(vehicleRepository.GetVehicleById(vehicleId) ?? throw new InvalidOperationException());

      vehicleRepository.VehicleChangedStream
                       .Where(vehicle => vehicle.Id == vehicleId)
                       .Subscribe(UpdatePropertiesOnSubscribe);

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

    public ReactiveProperty<int> MaximumSpeedStep { get; } = new();

    private void UpdatePropertiesOnSubscribe(Vehicle updatedVehicle)
    {
      Vehicle.Value = updatedVehicle;
      MaximumSpeedStep.Value = (int)updatedVehicle.RegulationStep;
    }
  }
}