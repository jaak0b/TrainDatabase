using System.IO.Compression;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;

namespace TrainDatabase.Infrastructure.Import;

/// <summary>
/// Imports a Roco/Fleischmann Z21 app export (<c>.z21</c> archive) into the database:
/// extracts the embedded SQLite file and vehicle images, then maps vehicles and functions
/// into the app's entities. Replaces the prior <c>Z21NewDatabaseImporter</c>, now using the
/// <see cref="IAppStorage"/> port and an injected <see cref="ILogger"/> instead of static
/// configuration and Serilog.
/// </summary>
public sealed class Z21DatabaseImporter(
    TrainDbContext database,
    IAppStorage appStorage,
    ILogger<Z21DatabaseImporter> logger) : IDatabaseImporter
{
    public async Task ImportAsync(string z21FilePath)
    {
        if (!string.Equals(Path.GetExtension(z21FilePath), ".z21", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Importing a '{Path.GetExtension(z21FilePath)}' file is not supported.");
        }

        database.DeleteAll();

        string sqliteFile = ExtractArchive(z21FilePath);
        await using SqliteConnection connection = new($"Data Source={sqliteFile}");
        await connection.OpenAsync();
        await MapVehiclesAsync(connection);
        await MapFunctionsAsync(connection, removeEmptyFunctions: true);
        await connection.CloseAsync();
    }

    private async Task MapVehiclesAsync(SqliteConnection connection)
    {
        List<VehicleEntity> vehicles = connection.Query<VehicleDto>("SELECT * FROM vehicles")
            .Select(dto => new VehicleEntity
            {
                Id = (int)dto.id,
                Name = dto.name,
                ImageName = dto.image_name,
                Type = (VehicleType)(int)dto.type,
                MaxSpeed = dto.max_speed,
                RegulationStep = GetRegulationStep(dto.speed_display),
                Address = dto.address,
                IsActive = dto.active == 1,
                Position = dto.position,
                FullName = dto.full_name,
                Railway = dto.railway,
                InvertTraction = dto.traction_direction == 1,
                Description = dto.description,
                Dummy = dto.dummy == 1,
            })
            .ToList();

        await database.AddRangeAsync(vehicles);
        await database.SaveChangesAsync();

        foreach (VehicleEntity vehicle in vehicles.Where(v => string.IsNullOrWhiteSpace(v.Name)))
        {
            logger.LogWarning("Imported vehicle with address {Address} has no display name.", vehicle.Address);
        }
    }

    private async Task MapFunctionsAsync(SqliteConnection connection, bool removeEmptyFunctions)
    {
        List<VehicleFunctionEntity> functions = connection.Query<FunctionDto>("SELECT * FROM functions")
            .Select(dto => new VehicleFunctionEntity
            {
                Id = (int)dto.id,
                Vehicle = database.Vehicles.FirstOrDefault(v => v.Id == (int)dto.vehicle_id),
                VehicleId = (int)dto.vehicle_id,
                ButtonType = (ButtonType)(int)dto.button_type,
                Name = string.IsNullOrWhiteSpace(dto.shortcut) ? dto.image_name : dto.shortcut,
                Time = (int)decimal.Parse(dto.time),
                Position = (int)dto.position,
                ImageName = dto.image_name,
                Address = (int)dto.function,
                ShowFunctionNumber = dto.show_function_number == 1,
                IsConfigured = dto.is_configured == 1,
            })
            .Where(function => function.Vehicle is not null)
            .Where(function => !removeEmptyFunctions || function.Name != "Empty")
            .ToList();

        await database.AddRangeAsync(functions);
        await database.SaveChangesAsync();
        database.InvokeCollectionChanged();
    }

    private static RegulationStep GetRegulationStep(long speedStep) => speedStep switch
    {
        14 => RegulationStep.Step14,
        28 => RegulationStep.Step28,
        _ => RegulationStep.Step128,
    };

    /// <summary>Extracts the archive into a temp folder and returns the embedded SQLite file path.</summary>
    private string ExtractArchive(string z21Path)
    {
        string tempPath = appStorage.CreateTempDirectory();

        string imageDirectory = appStorage.VehicleImageDirectory;
        if (Directory.Exists(imageDirectory))
        {
            Directory.Delete(imageDirectory, recursive: true);
        }

        Directory.CreateDirectory(imageDirectory);

        // The .z21 file is a renamed zip archive.
        string zipPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(z21Path) + ".zip");
        File.Copy(z21Path, zipPath);
        ZipFile.ExtractToDirectory(zipPath, tempPath);
        File.Delete(zipPath);

        string firstLayer = Directory.GetDirectories(tempPath).FirstOrDefault()
            ?? throw new InvalidOperationException($"Archive layout unexpected: no folder under '{tempPath}'.");
        string secondLayer = Directory.GetDirectories(firstLayer).FirstOrDefault()
            ?? throw new InvalidOperationException($"Archive layout unexpected: no folder under '{firstLayer}'.");

        List<string> files = Directory.GetFiles(secondLayer).ToList();
        string sqliteFile = files.FirstOrDefault(f => string.Equals(Path.GetExtension(f), ".sqlite", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("The .z21 archive did not contain a .sqlite database.");

        foreach (string image in files.Where(f => !string.Equals(Path.GetExtension(f), ".sqlite", StringComparison.OrdinalIgnoreCase)))
        {
            File.Move(image, Path.Combine(imageDirectory, Path.GetFileName(image)));
        }

        return sqliteFile;
    }
}
