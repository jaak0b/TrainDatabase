using TrainDatabase.Core.Domain;

namespace TrainDatabase.Infrastructure.Entities;

public partial class VehicleFunctionEntity : BaseObjectEntity
{
    public VehicleEntity? Vehicle { get; set; }

    public int VehicleId { get; set; }

    public ButtonType ButtonType { get; set; }

    public bool IsActive { get; set; } = true;

    public string Name { get; set; } = "";

    public int Time { get; set; }

    public int Position { get; set; }

    public string ImageName { get; set; } = "";

    public int Address { get; set; }

    public bool ShowFunctionNumber { get; set; }

    public bool IsConfigured { get; set; }

    public FunctionType EnumType { get; set; }

    public override bool Equals(object? obj) => obj is VehicleFunctionEntity function && Id == function.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"F{Address} {Name}";
}
