using Infrastructure;
using Persistence;
using System;
using Persistence.Database;
using Persistence.Entities;

namespace Core
{
  public class VehicleService
  {
    public VehicleService(Database db)
    {
      Db = db;
    }

    public Database Db { get; set; }

    public void AddTractionVehilce(VehicleEntity vehicle1, VehicleEntity vehicle2)
    {
      vehicle1.TractionVehicleIds.Add(vehicle2.Id);
      vehicle2.TractionVehicleIds.Add(vehicle1.Id);
      Db.Update(vehicle1);
      Db.Update(vehicle2);
    }

    public void RemoveTractionVehilce(VehicleEntity vehicle1, VehicleEntity vehicle2)
    {
      vehicle1.TractionVehicleIds.Remove(vehicle2.Id);
      vehicle2.TractionVehicleIds.Remove(vehicle1.Id);
      Db.Update(vehicle1);
      Db.Update(vehicle2);
    }
  }
}