using Autofac;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TrainDatabase.Core.Ports;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Hardware;
using TrainDatabase.Infrastructure.Mapping;
using TrainDatabase.Infrastructure.Platform;
using TrainDatabase.Infrastructure.Repositories;
using Z21.Autofac;

namespace TrainDatabase.Infrastructure;

/// <summary>
/// Registers the Infrastructure implementations of the Core ports (persistence, mapping,
/// command station, speed sensor, platform storage). This is the Desktop-flavoured module;
/// Android/Browser heads will override the platform-specific registrations.
/// </summary>
public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Platform storage / settings (Desktop).
        builder.RegisterType<DesktopAppStorage>().As<IAppStorage>().SingleInstance();
        builder.Register(c => new JsonSettingsStore(SettingsPath(c.Resolve<IAppStorage>())))
            .As<ISettingsStore>().SingleInstance();
        builder.RegisterType<FileVehicleImageStore>().As<IVehicleImageStore>().SingleInstance();

        // Mapping (Mapster).
        builder.Register(_ => MappingConfig.Create()).AsSelf().SingleInstance();
        builder.Register(c => new MapsterEntityMapper(c.Resolve<TypeAdapterConfig>()))
            .As<IEntityMapper>().SingleInstance();

        // Persistence.
        builder.Register(c =>
        {
            IAppStorage storage = c.Resolve<IAppStorage>();
            string dbPath = storage.DatabaseFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            DbContextOptions<TrainDbContext> options = new DbContextOptionsBuilder<TrainDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            return new TrainDbContext(options);
        }).AsSelf().SingleInstance();

        builder.RegisterType<VehicleRepository>().As<IVehicleRepository>().SingleInstance();
        builder.RegisterType<DatabaseInitializer>().As<IDatabaseInitializer>().SingleInstance();
        builder.RegisterType<Import.Z21DatabaseImporter>().As<IDatabaseImporter>().SingleInstance();

        // Command station (z21).
        builder.AddZ21();
        builder.RegisterType<Z21ClientAdapter>().As<IClientAdapter>().SingleInstance();

        // Speed sensor + serial devices.
        builder.RegisterType<SerialDeviceProvider>().As<ISerialDeviceProvider>().SingleInstance();
        builder.RegisterType<ArduinoSpeedSensorAdapter>().As<ISpeedSensorPort>().InstancePerDependency();
        builder.Register<SpeedSensorPortFactory>(context =>
        {
            IComponentContext c = context.Resolve<IComponentContext>();
            return (portName, baudRate) => c.Resolve<ISpeedSensorPort>(
                new TypedParameter(typeof(string), portName),
                new TypedParameter(typeof(int), baudRate));
        }).SingleInstance();
    }

    private static string SettingsPath(IAppStorage storage)
    {
        // DatabaseFilePath = <base>/Data/Database.sqlite → settings live in <base>.
        string dataDirectory = Path.GetDirectoryName(storage.DatabaseFilePath)!;
        string baseDirectory = Path.GetDirectoryName(dataDirectory)!;
        return Path.Combine(baseDirectory, "settings.json");
    }
}
