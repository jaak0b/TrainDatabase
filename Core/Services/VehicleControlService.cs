using System.Threading.Tasks;
using Core.Model;
using Persistence.Model;

namespace Core.Services
{
  public interface IVehicleControlService
  {
    Task SetVehicleSpeedAsync(Vehicle vehicle, int speed, bool direction);
  }

  public class VehicleControlService(IClientAdapter client) : IVehicleControlService
  {
    public async Task SetVehicleSpeedAsync(Vehicle vehicle, int speed, bool direction)
    {
      await client.SetVehiclesDriveAsync(new LocoSetDriveData() { VehicleAddress = (ushort)vehicle.Address, Direction = direction, Speed = (ushort)speed });
    }
  }
}