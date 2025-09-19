using System.Collections.Generic;

namespace Core.Model
{
  public class VehicleFunctionData : VehicleBaseData
  {
    public required Dictionary<ushort, bool> FunctionState { get; init; }
  }
}