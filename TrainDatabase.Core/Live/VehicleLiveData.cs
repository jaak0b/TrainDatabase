namespace TrainDatabase.Core.Live;

/// <summary>
/// Live speed/direction state for a vehicle, pushed from the command station.
/// </summary>
public class VehicleLiveData : VehicleBaseData
{
    public required int Speed { get; init; }

    public required bool Direction { get; init; }
}
