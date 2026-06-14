namespace TrainDatabase.Core.Services;

/// <summary>Snapshot of a long-running service (e.g. speed calibration) state.</summary>
public class ServiceState
{
    public required bool IsRunning { get; init; }

    public required int? CurrentVehicleId { get; init; }

    public static ServiceState Running(int vehicleId) => new() { IsRunning = true, CurrentVehicleId = vehicleId };

    public static ServiceState Idle() => new() { IsRunning = false, CurrentVehicleId = null };
}
