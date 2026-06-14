namespace TrainDatabase.Infrastructure.Entities;

/// <summary>
/// Join row coupling a lead vehicle to one member vehicle in a multi-traction consist.
/// Foreign keys cascade, so deleting either vehicle removes the membership automatically.
/// </summary>
public class VehicleTractionEntity
{
    public int LeadVehicleId { get; set; }

    public VehicleEntity Lead { get; set; } = null!;

    public int MemberVehicleId { get; set; }

    public VehicleEntity Member { get; set; } = null!;
}
