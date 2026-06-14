namespace TrainDatabase.Core.Live;

/// <summary>
/// Live function (F0..Fn) on/off state for a vehicle, pushed from the command station.
/// </summary>
public class VehicleFunctionData : VehicleBaseData
{
    public required IReadOnlyDictionary<ushort, bool> FunctionState { get; init; }
}
