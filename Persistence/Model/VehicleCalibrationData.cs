namespace Persistence.Model
{
  public class VehicleCalibrationData : BaseObject
  {
    public Vehicle Vehicle { get; set; }

    public int VehicleId { get; set; }

    public bool Direction { get; set; }
    
    public int SpeedStep { get; set; }

    public decimal MeasuredSpeed { get; set; }
  }
}