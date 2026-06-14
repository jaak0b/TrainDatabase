using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Infrastructure.Mapping;

namespace TrainDatabase.Infrastructure.Repositories;

public class VehicleRepository(TrainDbContext database, IEntityMapper mapper) : IVehicleRepository
{
    private readonly Subject<Vehicle> vehicleUpdatedSubject = new();

    public IObservable<Vehicle> VehicleChangedStream => vehicleUpdatedSubject;

    public Vehicle GetVehicleByIdRequired(int vehicleId) =>
        GetVehicleById(vehicleId) ?? throw new IdNotFoundException($"Vehicle with ID {vehicleId} not found.");

    public Vehicle? GetVehicleById(int vehicleId)
    {
        VehicleEntity? entity = database.Vehicles
            .Include(vehicle => vehicle.TractionMembers)
            .FirstOrDefault(vehicle => vehicle.Id == vehicleId);
        return entity is null ? null : mapper.Map<Vehicle>(entity);
    }

    public IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString)
    {
        string search = searchString ?? "";
        return database.Vehicles
            .Include(vehicle => vehicle.TractionMembers)
            .ToList()
            .Where(entity => Contains(entity.Name, search) || Contains(entity.FullName, search))
            .Select(mapper.Map<Vehicle>)
            .ToList();
    }

    public void UpdateVehiclePositions(IReadOnlyList<VehiclePosition> updates)
    {
        foreach (VehiclePosition update in updates)
        {
            VehicleEntity? entity = database.Vehicles.Find(update.VehicleId);
            if (entity is not null)
            {
                entity.Position = update.Position;
            }
        }

        database.SaveChanges();
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
        VehicleEntity entity = await database.Vehicles
            .Include(candidate => candidate.TractionMembers)
            .FirstOrDefaultAsync(candidate => candidate.Id == vehicle.Id)
            ?? throw new IdNotFoundException($"Vehicle with ID {vehicle.Id} not found.");

        mapper.Map(vehicle, entity);
        SyncTractionMembers(entity, vehicle.TractionVehicleIds);
        await database.SaveChangesAsync();

        NotifyVehicleUpdated(vehicle.Id);
    }

    private static void SyncTractionMembers(VehicleEntity lead, IEnumerable<int> memberIds)
    {
        HashSet<int> desired = memberIds.Where(id => id != lead.Id).ToHashSet();
        lead.TractionMembers.RemoveAll(member => !desired.Contains(member.MemberVehicleId));
        foreach (int id in desired.Where(id => lead.TractionMembers.All(member => member.MemberVehicleId != id)))
        {
            lead.TractionMembers.Add(new VehicleTractionEntity { LeadVehicleId = lead.Id, MemberVehicleId = id });
        }
    }

    public async Task<int> AddVehicleAsync(Vehicle vehicle)
    {
        VehicleEntity entity = mapper.Map<VehicleEntity>(vehicle);
        entity.Id = 0;
        await database.Vehicles.AddAsync(entity);
        await database.SaveChangesAsync();

        if (vehicle.TractionVehicleIds.Count > 0)
        {
            SyncTractionMembers(entity, vehicle.TractionVehicleIds);
            await database.SaveChangesAsync();
        }

        database.InvokeCollectionChanged();
        return entity.Id;
    }

    public async Task UpdateVehicleFunctionsAsync(int vehicleId, IReadOnlyList<VehicleFunction> functions)
    {
        foreach (VehicleFunction function in functions)
        {
            VehicleFunctionEntity? entity = await database.Functions.FindAsync(function.Id);
            if (entity is not null)
            {
                entity.Name = function.Name;
                entity.Address = function.Address;
                entity.ButtonType = function.ButtonType;
                entity.EnumType = function.EnumType;
                entity.IsActive = function.IsActive;
            }
        }

        await database.SaveChangesAsync();
        NotifyVehicleUpdated(vehicleId);
    }

    public async Task DeleteVehicleAsync(int vehicleId)
    {
        VehicleEntity? entity = await database.Vehicles.FindAsync(vehicleId);
        if (entity is null)
        {
            return;
        }

        database.Vehicles.Remove(entity);
        await database.SaveChangesAsync();
        database.InvokeCollectionChanged();
    }

    public void RevertVehicleChange(int vehicleId) => NotifyVehicleUpdated(vehicleId);

    public void NotifyVehicleUpdated(int vehicleId) => vehicleUpdatedSubject.OnNext(GetVehicleByIdRequired(vehicleId));

    private static bool Contains(string? value, string search) =>
        value?.Contains(search, StringComparison.InvariantCultureIgnoreCase) == true;
}
