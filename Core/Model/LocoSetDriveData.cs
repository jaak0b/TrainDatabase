namespace Core.Model
{
  public class LocoSetDriveData
  {
    public ushort VehicleAddress { get; init; }

    public ushort Speed { get; init; }

    public bool Direction { get; set; }
  }
}