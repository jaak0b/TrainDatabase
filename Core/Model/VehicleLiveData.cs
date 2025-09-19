namespace Core.Model
{
  public class VehicleLiveData : VehicleBaseData
  {
    public required int Speed { get; init; }

    public required bool Direction { get; init; }
  }
}