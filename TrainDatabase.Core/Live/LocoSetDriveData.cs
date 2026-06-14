using TrainDatabase.Core.Domain;

namespace TrainDatabase.Core.Live;

/// <summary>
/// A command to drive a single vehicle at a given speed and direction.
/// </summary>
public class LocoSetDriveData
{
    public ushort VehicleAddress { get; init; }

    public ushort Speed { get; init; }

    public bool Direction { get; init; }

    public RegulationStep SpeedStep { get; init; } = RegulationStep.Step128;
}
