using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoMapper;
using Persistence.Entities;
using Persistence.Extensions;
using Persistence.Model;
using Persistence.Ports;

namespace Persistence.Repositories
{
  public class VehicleRepository(Database.Database database, IMapper mapper) : IVehicleRepository
  {

    private readonly Subject<Vehicle> vehicleUpdatedSubject = new();

    public IObservable<Vehicle> VehicleChangedStream => vehicleUpdatedSubject;

    public Vehicle GetVehicleByIdRequired(int vehicleId) => GetVehicleById(vehicleId) ?? throw new IdNotFoundException();

    public Vehicle? GetVehicleById(int vehicleId)
    {
      VehicleEntity? vehicleEntity = database.Vehicles.Find(vehicleId);
      return vehicleEntity is null ? null : mapper.Map<VehicleEntity, Vehicle>(vehicleEntity);
    }

    public IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString)
    {
      string searchStringNotNull = searchString ?? "";
      return database.Vehicles
                     .ToList()
                     .Where(entity => Contain(entity.Name, searchStringNotNull) || Contain(entity.FullName, searchStringNotNull))
                     .Select(mapper.Map<VehicleEntity, Vehicle>)
                     .ToList();
    }

    public void UpdateVehiclePositions(IEnumerable<(int vehicleId, int position)> updates)
    {
      foreach ((int vehicleId, int position) in updates)
      {
        VehicleEntity? entity = database.Vehicles.Find(vehicleId);
        if (entity != null)
        {
          entity.Position = position;
        }
      }

      database.SaveChanges();
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
      VehicleEntity? entity = await database.Vehicles.FindAsync(vehicle.Id);
      if (entity == null)
        throw new KeyNotFoundException($"Vehicle with ID {vehicle.Id} not found.");

      mapper.Map(vehicle, entity);

      await database.SaveChangesAsync();

      NotifyVehicleUpdated(vehicle.Id);
    }

    public void RevertVehicleChange(int vehicleId)
    {
      NotifyVehicleUpdated(vehicleId);
    }

    private static bool Contain(object value, string searchString) => value?.ToString()?.Contains(searchString, StringComparison.InvariantCultureIgnoreCase) == true;

    public void NotifyVehicleUpdated(int vehicleId)
    {
      Vehicle updated = GetVehicleByIdRequired(vehicleId);
      vehicleUpdatedSubject.OnNext(updated);
    }
  }
}