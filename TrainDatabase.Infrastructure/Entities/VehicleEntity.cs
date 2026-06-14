using System.ComponentModel.DataAnnotations;
using TrainDatabase.Core.Domain;

namespace TrainDatabase.Infrastructure.Entities;

public partial class VehicleEntity : BaseObjectEntity, IEquatable<VehicleEntity>, IComparable
{
    [Required]
    public string Name { get; set; } = "";

    public string ImageName { get; set; } = "";

    public VehicleType Type { get; set; } = VehicleType.Lokomotive;

    public long? MaxSpeed { get; set; } = 0;

    public RegulationStep RegulationStep { get; set; } = RegulationStep.Step128;

    [Required]
    public long Address { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    public long Position { get; set; } = 0;

    public string FullName { get; set; } = "";

    public string Railway { get; set; } = "";

    public bool InvertTraction { get; set; }

    public string Description { get; set; } = "";

    public bool? Dummy { get; set; } = false;

    public List<VehicleFunctionEntity> Functions { get; set; } = new();

    public List<VehicleCalibrationDataEntity> VehicleCalibrations { get; set; } = new();

    [Obsolete]
    public decimal?[] TractionForward { get; set; } = new decimal?[DccConstants.MaxDccStep + 1];

    [Obsolete]
    public decimal?[] TractionBackward { get; set; } = new decimal?[DccConstants.MaxDccStep + 1];

    public List<VehicleTractionEntity> TractionMembers { get; set; } = new();

    public int CompareTo(object? obj) => Id.CompareTo((obj as VehicleEntity)?.Id ?? 0);

    public bool Equals(VehicleEntity? other) => Id == other?.Id;

    public override bool Equals(object? obj) => obj is VehicleEntity other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets the real-world speed for a given speed step and direction.
    /// </summary>
    public decimal? GetTractionSpeed(int speed, bool direction)
    {
#pragma warning disable CS0612 // obsolete traction arrays retained for legacy data
        return speed <= DccConstants.MaxDccStep && speed >= 2
            ? direction ? TractionForward[speed] : TractionBackward[speed]
            : throw new ArgumentOutOfRangeException(nameof(speed));
#pragma warning restore CS0612
    }

    public override string ToString() =>
        $"Add: {Address} - Name: \"{(string.IsNullOrEmpty(Name) ? FullName : Name)}\" - Pos: {Position} - {(IsActive ? "Aktiv" : "Deaktiviert")}";
}
