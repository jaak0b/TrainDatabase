#nullable disable

using Persistence.Enums;

namespace Persistence.Entities
{
  public partial class VehicleFunctionEntity
  {
    public int Id { get; set; }

    public VehicleEntity Vehicle { get; set; }

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

    override public bool Equals(object obj)
    {
      return obj is VehicleFunctionEntity function && Id == function?.Id;
    }

    override public string ToString()
    {
      return $"F{Address} {Name}";
    }

    override public int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}