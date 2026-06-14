using System.Reactive.Subjects;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.UnitTest.Fakes;

/// <summary>In-memory <see cref="IVehicleRepository"/> for testing services that resolve vehicles by id.</summary>
public sealed class FakeVehicleRepository : IVehicleRepository
{
    private readonly Dictionary<int, Vehicle> vehicles = new();
    private readonly Subject<Vehicle> changed = new();

    public IObservable<Vehicle> VehicleChangedStream => changed;

    public void Seed(params Vehicle[] toSeed)
    {
        foreach (Vehicle vehicle in toSeed)
        {
            vehicles[vehicle.Id] = vehicle;
        }
    }

    public Vehicle GetVehicleByIdRequired(int vehicleId) =>
        vehicles.TryGetValue(vehicleId, out Vehicle? vehicle)
            ? vehicle
            : throw new IdNotFoundException($"No vehicle with id {vehicleId}.");

    public Vehicle? GetVehicleById(int vehicleId) =>
        vehicles.TryGetValue(vehicleId, out Vehicle? vehicle) ? vehicle : null;

    public IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString) => vehicles.Values.ToList();

    public void UpdateVehiclePositions(IEnumerable<(int vehicleId, int position)> updates)
    {
    }

    public Task UpdateVehicleAsync(Vehicle vehicle)
    {
        vehicles[vehicle.Id] = vehicle;
        changed.OnNext(vehicle);
        return Task.CompletedTask;
    }

    public Task<int> AddVehicleAsync(Vehicle vehicle)
    {
        vehicles[vehicle.Id] = vehicle;
        return Task.FromResult(vehicle.Id);
    }

    public Task UpdateVehicleFunctionsAsync(int vehicleId, IReadOnlyList<VehicleFunction> functions) => Task.CompletedTask;

    public Task DeleteVehicleAsync(int vehicleId)
    {
        vehicles.Remove(vehicleId);
        return Task.CompletedTask;
    }

    public void RevertVehicleChange(int vehicleId)
    {
    }
}
