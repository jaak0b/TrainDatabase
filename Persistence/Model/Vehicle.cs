using System.Collections.Generic;
using Persistence.Enums;
using Z21;

namespace Persistence.Model
{
  public class Vehicle : BaseObject
  {
    public string Name { get; set; } = "";

    public string ImageName { get; set; } = "";

    public VehicleType Type { get; set; } = VehicleType.Lokomotive;

    public long? MaxSpeed { get; set; } = 0;

    public long Speedstep { get; set; } = 128;

    public long Address { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    public long Position { get; set; } = 0;

    public string FullName { get; set; } = "";

    public string Railway { get; set; } = "";

    public bool InvertTraction { get; set; }

    public string Description { get; set; } = "";

    public bool? Dummy { get; set; } = false;

    public List<VehicleFunction> Functions { get; set; } = new();

    public decimal?[] TractionForward { get; set; } = new decimal?[Client.maxDccStep + 1];

    public decimal?[] TractionBackward { get; set; } = new decimal?[Client.maxDccStep + 1];

    public List<int> TractionVehicleIds { get; set; } = new();
  }
}