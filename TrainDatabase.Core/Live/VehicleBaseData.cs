namespace TrainDatabase.Core.Live;

/// <summary>
/// Base class for live data messages keyed by a vehicle's DCC address.
/// </summary>
public abstract class VehicleBaseData
{
    public required ushort VehicleAddress { get; init; }
}
