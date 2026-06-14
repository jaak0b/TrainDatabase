using System.Reactive.Linq;
using System.Reactive.Subjects;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.Presenters;

public interface IVehiclePresenter
{
    IObservableValue<Vehicle> Vehicle { get; }

    IObservableValue<int> MaximumSpeedStep { get; }

    IObservableValue<int> Speed { get; }

    IObservableValue<bool> Direction { get; }

    /// <summary>Live function on/off states keyed by function index, pushed from the command station.</summary>
    IObservable<IReadOnlyDictionary<ushort, bool>> FunctionStates { get; }
}

public delegate IVehiclePresenter VehiclePresenterFactory(int vehicleId);

/// <summary>
/// Reactive, per-vehicle view of live drive state combined with the persisted vehicle.
/// Subscribes to repository changes and live command-station data.
/// </summary>
public sealed class VehiclePresenter : IVehiclePresenter
{
    private readonly ObservableValue<Vehicle> vehicle;
    private readonly ObservableValue<int> maximumSpeedStep;
    private readonly ObservableValue<int> speed = new(0);
    private readonly ObservableValue<bool> direction = new(false);
    private readonly Subject<IReadOnlyDictionary<ushort, bool>> functionStates = new();

    public VehiclePresenter(int vehicleId, IVehicleRepository vehicleRepository, IClientAdapter client)
    {
        Vehicle initial = vehicleRepository.GetVehicleByIdRequired(vehicleId);
        vehicle = new ObservableValue<Vehicle>(initial);
        maximumSpeedStep = new ObservableValue<int>((int)initial.RegulationStep);

        vehicleRepository.VehicleChangedStream
            .Where(updated => updated.Id == vehicleId)
            .Subscribe(UpdateVehicle);

        client.VehicleData.Subscribe(OnVehicleData);
        client.VehicleFunctionData.Subscribe(OnFunctionData);
    }

    public IObservableValue<Vehicle> Vehicle => vehicle;

    public IObservableValue<int> MaximumSpeedStep => maximumSpeedStep;

    public IObservableValue<int> Speed => speed;

    public IObservableValue<bool> Direction => direction;

    public IObservable<IReadOnlyDictionary<ushort, bool>> FunctionStates => functionStates;

    private void OnFunctionData(VehicleFunctionData data)
    {
        if (data.VehicleAddress == vehicle.Value.Address)
        {
            functionStates.OnNext(data.FunctionState);
        }
    }

    private void UpdateVehicle(Vehicle updated)
    {
        vehicle.SetValue(updated);
        maximumSpeedStep.SetValue((int)updated.RegulationStep);
    }

    private void OnVehicleData(VehicleLiveData liveData)
    {
        if (liveData.VehicleAddress != vehicle.Value.Address)
        {
            return;
        }

        speed.SetValue(liveData.Speed);
        direction.SetValue(liveData.Direction);
    }
}
