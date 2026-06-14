using System.Net;
using System.Reactive;
using System.Reactive.Subjects;
using Autofac;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.UI.Browser;

// Browser is a deferred target: no raw UDP/serial, so hardware ports are no-ops and the
// catalogue is in-memory. Real persistence + a WebSocket control proxy are future work.

internal sealed class NullClientAdapter : IClientAdapter
{
    public void Connect(IPEndPoint endPoint) { }
    public IObservable<VehicleLiveData> VehicleData { get; } = new Subject<VehicleLiveData>();
    public IObservable<VehicleFunctionData> VehicleFunctionData { get; } = new Subject<VehicleFunctionData>();
    public IObservableValue<bool> IsConnected { get; } = new ObservableValue<bool>(false);
    public IObservableValue<TrackPower> TrackPower { get; } = new ObservableValue<TrackPower>(TrainDatabase.Core.Live.TrackPower.Off);
    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas) => Task.CompletedTask;
    public Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on) => Task.CompletedTask;
    public Task SetTrackPowerAsync(bool on) => Task.CompletedTask;
}

internal sealed class NullSerialDeviceProvider : ISerialDeviceProvider
{
    public IReadOnlyList<string> GetPortNames() => Array.Empty<string>();
    public IObservable<Unit> DeviceChanges { get; } = new Subject<Unit>();
}

internal sealed class InMemorySettingsStore : ISettingsStore
{
    private readonly Dictionary<string, string> values = new();
    public string? Get(string key) => values.TryGetValue(key, out string? v) ? v : null;
    public void Set(string key, string? value)
    {
        if (value is null) values.Remove(key); else values[key] = value;
    }
}

internal sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly Subject<Vehicle> changed = new();
    private readonly Dictionary<int, Vehicle> vehicles = new();

    public IObservable<Vehicle> VehicleChangedStream => changed;
    public Vehicle GetVehicleByIdRequired(int vehicleId) => GetVehicleById(vehicleId) ?? throw new IdNotFoundException();
    public Vehicle? GetVehicleById(int vehicleId) => vehicles.TryGetValue(vehicleId, out Vehicle? v) ? v : null;
    public IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString) => vehicles.Values.ToList();
    public void UpdateVehiclePositions(IReadOnlyList<VehiclePosition> updates) { }
    public Task UpdateVehicleAsync(Vehicle vehicle) { vehicles[vehicle.Id] = vehicle; changed.OnNext(vehicle); return Task.CompletedTask; }
    public Task<int> AddVehicleAsync(Vehicle vehicle)
    {
        int id = vehicles.Count == 0 ? 1 : vehicles.Keys.Max() + 1;
        vehicle.Id = id;
        vehicles[id] = vehicle;
        return Task.FromResult(id);
    }
    public Task DeleteVehicleAsync(int vehicleId) { vehicles.Remove(vehicleId); return Task.CompletedTask; }
    public Task UpdateVehicleFunctionsAsync(int vehicleId, IReadOnlyList<VehicleFunction> functions) => Task.CompletedTask;
    public void RevertVehicleChange(int vehicleId) { }
}

internal sealed class NullFilePicker : TrainDatabase.Presentation.Files.IFilePicker
{
    public Task<string?> PickFileAsync(string title, IReadOnlyList<string> extensions) => Task.FromResult<string?>(null);
}

internal sealed class NullDatabaseImporter : IDatabaseImporter
{
    public Task ImportAsync(string z21FilePath) => Task.CompletedTask;
}

internal sealed class NullVehicleImageStore : IVehicleImageStore
{
    public byte[]? TryGetImage(string imageName) => null;
}

/// <summary>Browser port implementations (deferred: stubs + in-memory catalogue).</summary>
public sealed class BrowserModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<NullClientAdapter>().As<IClientAdapter>().SingleInstance();
        builder.RegisterType<NullSerialDeviceProvider>().As<ISerialDeviceProvider>().SingleInstance();
        builder.RegisterType<InMemorySettingsStore>().As<ISettingsStore>().SingleInstance();
        builder.RegisterType<InMemoryVehicleRepository>().As<IVehicleRepository>().SingleInstance();
        builder.RegisterType<NullFilePicker>().As<TrainDatabase.Presentation.Files.IFilePicker>().SingleInstance();
        builder.RegisterType<NullDatabaseImporter>().As<IDatabaseImporter>().SingleInstance();
        builder.RegisterType<NullVehicleImageStore>().As<IVehicleImageStore>().SingleInstance();
    }
}
