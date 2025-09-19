using System;
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
    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, Z21.Client client)
    {
      Vehicle = vehicleRepository.GetVehicleById(vehicleId) ?? throw new InvalidOperationException();

      client.OnGetLocoInfo += Client_OnOnGetLocoInfo;
    }

    private void Client_OnOnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
    {
      if (Vehicle.Address != e.Data.Adresse.Value)
        return;

      Speed.Value = e.Data.Speed;
      Direction.Value = e.Data.DrivingDirection;
    }

    public Vehicle Vehicle { get; }

    public ReactiveProperty<int> Speed { get; } = new();

    public ReactiveProperty<bool> Direction { get; } = new();
  }
}