using Persistence.Entities;
using Persistence.Enums;

namespace Persistence.Model
{
  public class VehicleFunction
  {
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public ButtonType ButtonType { get; set; }

    public bool IsActive { get; set; } = true;

    public string Name { get; set; }

    public int Time { get; set; }

    public int Position { get; set; }

    public string ImageName { get; set; }

    public int Address { get; set; }

    public bool ShowFunctionNumber { get; set; }

    public bool IsConfigured { get; set; }

    public FunctionType EnumType { get; set; }
  }
}