using TrainDatabase.Core.Domain;

namespace TrainDatabase.Core.Ports;

/// <summary>
/// Read/write access to vehicles. Implemented in Infrastructure over EF Core;
/// returns domain <see cref="Vehicle"/> models (mapped from EF entities).
/// </summary>
public interface IVehicleRepository
{
    /// <summary>Emits the updated <see cref="Vehicle"/> whenever one changes.</summary>
    IObservable<Vehicle> VehicleChangedStream { get; }

    /// <summary>Gets the vehicle with <paramref name="vehicleId"/>.</summary>
    /// <exception cref="IdNotFoundException">No vehicle exists for the id.</exception>
    Vehicle GetVehicleByIdRequired(int vehicleId);

    /// <summary>Gets the vehicle with <paramref name="vehicleId"/>, or <c>null</c> if none exists.</summary>
    Vehicle? GetVehicleById(int vehicleId);

    /// <summary>Searches vehicles across most fields.</summary>
    IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString);

    void UpdateVehiclePositions(IEnumerable<(int vehicleId, int position)> updates);

    Task UpdateVehicleAsync(Vehicle vehicle);

    /// <summary>Adds a new vehicle and returns its generated id.</summary>
    Task<int> AddVehicleAsync(Vehicle vehicle);

    /// <summary>Updates the scalar fields of existing functions (name, address, type, active).</summary>
    Task UpdateVehicleFunctionsAsync(int vehicleId, IReadOnlyList<VehicleFunction> functions);

    /// <summary>Deletes the vehicle with the given id (no-op if it does not exist).</summary>
    Task DeleteVehicleAsync(int vehicleId);

    void RevertVehicleChange(int vehicleId);
}
