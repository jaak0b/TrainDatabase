using System.ComponentModel.DataAnnotations;

namespace Persistence.Entities
{
  public class VehicleCalibrationDataEntity : BaseObjectEntity
  {
    [Required]
    public VehicleEntity Vehicle { get; set; }

    public int VehicleId { get; set; }

    public bool Direction { get; set; }
    
    public int SpeedStep { get; set; }

    public decimal MeasuredSpeed { get; set; }
  }
}