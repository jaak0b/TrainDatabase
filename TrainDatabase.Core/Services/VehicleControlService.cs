using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.Services;

public interface IVehicleControlService
{
    Task SetVehicleSpeedAsync(Vehicle vehicle, int speed, bool direction);

    Task SetVehicleFunctionAsync(Vehicle vehicle, int functionIndex, bool on);
}

public class VehicleControlService(IClientAdapter client, IVehicleRepository repository) : IVehicleControlService
{
    public Task SetVehicleSpeedAsync(Vehicle vehicle, int speed, bool direction)
    {
        List<LocoSetDriveData> commands = new()
        {
            new LocoSetDriveData
            {
                VehicleAddress = (ushort)vehicle.Address,
                Direction = direction,
                Speed = (ushort)speed,
                SpeedStep = vehicle.RegulationStep,
            },
        };

        foreach (int memberId in vehicle.TractionVehicleIds)
        {
            Vehicle? member = repository.GetVehicleById(memberId);
            if (member is null)
            {
                continue;
            }

            commands.Add(new LocoSetDriveData
            {
                VehicleAddress = (ushort)member.Address,
                Direction = member.InvertTraction ? !direction : direction,
                Speed = (ushort)speed,
                SpeedStep = member.RegulationStep,
            });
        }

        return client.SetVehiclesDriveAsync(commands.ToArray());
    }

    public Task SetVehicleFunctionAsync(Vehicle vehicle, int functionIndex, bool on) =>
        client.SetVehicleFunctionAsync((ushort)vehicle.Address, (ushort)functionIndex, on);
}
