using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Reactive;
using TrainDatabase.Core.Services;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Presentation.UnitTest.Fakes;

public sealed class FakeVehiclePresenter : IVehiclePresenter
{
    public ObservableValue<Vehicle> VehicleValue { get; }
    public ObservableValue<int> MaximumSpeedStepValue { get; } = new(128);
    public ObservableValue<int> SpeedValue { get; } = new(0);
    public ObservableValue<bool> DirectionValue { get; } = new(false);

    public FakeVehiclePresenter(Vehicle vehicle) => VehicleValue = new ObservableValue<Vehicle>(vehicle);

    public System.Reactive.Subjects.Subject<IReadOnlyDictionary<ushort, bool>> FunctionStatesSubject { get; } = new();

    public IObservableValue<Vehicle> Vehicle => VehicleValue;
    public IObservableValue<int> MaximumSpeedStep => MaximumSpeedStepValue;
    public IObservableValue<int> Speed => SpeedValue;
    public IObservableValue<bool> Direction => DirectionValue;
    public IObservable<IReadOnlyDictionary<ushort, bool>> FunctionStates => FunctionStatesSubject;
}

public sealed class FakeClientPresenter : IClientPresenter
{
    public ObservableValue<bool> IsConnectedValue { get; } = new(true);
    public ObservableValue<bool> IsDisconnectedValue { get; } = new(false);

    public IObservableValue<bool> IsConnected => IsConnectedValue;
    public IObservableValue<bool> IsDisconnected => IsDisconnectedValue;
}

public sealed class FakeVehicleControlService : IVehicleControlService
{
    public List<(int Speed, bool Direction)> Calls { get; } = new();
    public List<(int Function, bool On)> FunctionCalls { get; } = new();

    public Task SetVehicleSpeedAsync(Vehicle vehicle, int speed, bool direction)
    {
        Calls.Add((speed, direction));
        return Task.CompletedTask;
    }

    public Task SetVehicleFunctionAsync(Vehicle vehicle, int functionIndex, bool on)
    {
        FunctionCalls.Add((functionIndex, on));
        return Task.CompletedTask;
    }
}

public sealed class FakeVehicleRepository : IVehicleRepository
{
    private readonly Dictionary<int, Vehicle> vehicles = new();
    private readonly System.Reactive.Subjects.Subject<Vehicle> changed = new();

    public IObservable<Vehicle> VehicleChangedStream => changed;

    public Vehicle GetVehicleByIdRequired(int vehicleId) =>
        GetVehicleById(vehicleId) ?? throw new IdNotFoundException();

    public Vehicle? GetVehicleById(int vehicleId) =>
        vehicles.TryGetValue(vehicleId, out Vehicle? v) ? v : new Vehicle { Id = vehicleId, Address = vehicleId };

    public IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString) => vehicles.Values.ToList();

    public void UpdateVehiclePositions(IEnumerable<(int vehicleId, int position)> updates) { }

    public Task UpdateVehicleAsync(Vehicle vehicle)
    {
        vehicles[vehicle.Id] = vehicle;
        changed.OnNext(vehicle);
        return Task.CompletedTask;
    }

    public Task<int> AddVehicleAsync(Vehicle vehicle)
    {
        int id = vehicles.Count == 0 ? 1 : vehicles.Keys.Max() + 1;
        vehicle.Id = id;
        vehicles[id] = vehicle;
        return Task.FromResult(id);
    }

    public Task DeleteVehicleAsync(int vehicleId)
    {
        vehicles.Remove(vehicleId);
        return Task.CompletedTask;
    }

    public List<(int VehicleId, int FunctionCount)> FunctionUpdates { get; } = new();

    public Task UpdateVehicleFunctionsAsync(int vehicleId, IReadOnlyList<VehicleFunction> functions)
    {
        FunctionUpdates.Add((vehicleId, functions.Count));
        return Task.CompletedTask;
    }

    public void RevertVehicleChange(int vehicleId) { }
}

public sealed class FakeVehicleImageStore : IVehicleImageStore
{
    public byte[]? TryGetImage(string imageName) => null;
}

public sealed class FakeFilePicker(string? path) : TrainDatabase.Presentation.Files.IFilePicker
{
    public Task<string?> PickFileAsync(string title, IReadOnlyList<string> extensions) => Task.FromResult(path);
}

public sealed class FakeDatabaseImporter : IDatabaseImporter
{
    public List<string> Imported { get; } = new();
    public Task ImportAsync(string z21FilePath)
    {
        Imported.Add(z21FilePath);
        return Task.CompletedTask;
    }
}

public sealed class FakeDialogService : TrainDatabase.Presentation.Dialogs.IDialogService
{
    public bool ConfirmResult { get; set; } = true;
    public List<string> Alerts { get; } = new();

    public Task AlertAsync(string title, string message)
    {
        Alerts.Add($"{title}: {message}");
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message) => Task.FromResult(ConfirmResult);
}

public sealed class FakeSettingsStore : ISettingsStore
{
    private readonly Dictionary<string, string> values = new();

    public string? Get(string key) => values.TryGetValue(key, out string? v) ? v : null;

    public void Set(string key, string? value)
    {
        if (value is null)
        {
            values.Remove(key);
        }
        else
        {
            values[key] = value;
        }
    }
}
