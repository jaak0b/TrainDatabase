namespace Core.Model
{
  public class ServiceState
  {
    public static ServiceState Running(int vehicleId)
    {
      return new() { IsRunning = true, CurrentVehicleId = vehicleId };
    }

    public static ServiceState Idle()
    {
      return new() { IsRunning = false, CurrentVehicleId = null };
    }

    public required bool IsRunning { get; init; }

    public required int? CurrentVehicleId { get; init; }
  }
}