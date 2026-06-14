using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainDatabase.Core.Ports;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;

namespace TrainDatabase.Infrastructure.IntegrationTest;

[TestFixture]
public class DatabaseInitializerTests
{
    [Test]
    public async Task InitializeAsync_AppliesAllMigrations_OnFreshDatabase()
    {
        using TempDatabase db = new(initialize: false);
        IDatabaseInitializer initializer = new DatabaseInitializer(db.Context);

        await initializer.InitializeAsync();

        Assert.That(db.Context.Database.GetPendingMigrations(), Is.Empty);
    }

    [Test]
    public async Task InitializeAsync_CreatesSchema_IncludingCalibrationTableAndUniqueIndex()
    {
        using TempDatabase db = new(initialize: false);
        await new DatabaseInitializer(db.Context).InitializeAsync();

        VehicleEntity vehicle = new() { Name = "Test", Address = 3 };
        db.Context.Vehicles.Add(vehicle);
        db.Context.SaveChanges();

        db.Context.VehicleCalibrationData.Add(new VehicleCalibrationDataEntity
        {
            VehicleId = vehicle.Id,
            Direction = true,
            SpeedStep = 10,
            MeasuredSpeed = 1.5m,
        });
        db.Context.SaveChanges();

        // The (VehicleId, Direction, SpeedStep) unique index must reject a duplicate.
        db.Context.VehicleCalibrationData.Add(new VehicleCalibrationDataEntity
        {
            VehicleId = vehicle.Id,
            Direction = true,
            SpeedStep = 10,
            MeasuredSpeed = 2.0m,
        });
        Assert.Throws<DbUpdateException>(() => db.Context.SaveChanges());
    }

    [Test]
    public async Task InitializeAsync_StampsBaseline_WhenSchemaAlreadyExists()
    {
        // Simulate a pre-rewrite database: the baseline schema is present, but its migration
        // history was never recorded (the rewrite re-homed the migrations). Initialization must
        // stamp the baseline and apply the remaining migrations without recreating baseline tables.
        using TempDatabase db = new(initialize: false);
        string baseline = db.Context.Database.GetMigrations().First();
        db.Context.GetService<IMigrator>().Migrate(baseline);
        db.Context.Database.ExecuteSqlRaw("DELETE FROM \"__EFMigrationsHistory\";");

        Assert.DoesNotThrowAsync(() => new DatabaseInitializer(db.Context).InitializeAsync());

        Assert.Multiple(() =>
        {
            Assert.That(db.Context.Database.GetPendingMigrations(), Is.Empty);
            Assert.That(db.Context.Database.GetAppliedMigrations(), Is.Not.Empty);
        });
    }
}
