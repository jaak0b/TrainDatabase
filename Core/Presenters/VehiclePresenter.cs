using System;
using Persistence.Model;
using Persistence.Ports;

namespace Core.Presenters
{
  public class VehiclePresenter
  {
    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, Z21.Client client)
    {
      Vehicle = vehicleRepository.GetVehicleById(vehicleId) ?? throw new InvalidOperationException();
    }

    public Vehicle Vehicle { get; }
  }
}