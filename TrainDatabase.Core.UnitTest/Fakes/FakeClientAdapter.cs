using System.Net;
using System.Reactive.Subjects;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.UnitTest.Fakes;

/// <summary>In-memory <see cref="IClientAdapter"/> for testing services and presenters.</summary>
public sealed class FakeClientAdapter : IClientAdapter
{
    public Subject<VehicleLiveData> VehicleDataSubject { get; } = new();
    public Subject<VehicleFunctionData> VehicleFunctionDataSubject { get; } = new();
    public ObservableValue<bool> IsConnectedValue { get; } = new(false);
    public ObservableValue<TrackPower> TrackPowerValue { get; } = new(Live.TrackPower.Off);

    public List<LocoSetDriveData> DriveCommands { get; } = new();
    public List<bool> TrackPowerCommands { get; } = new();
    public IPEndPoint? ConnectedEndPoint { get; private set; }

    public void Connect(IPEndPoint endPoint) => ConnectedEndPoint = endPoint;

    public IObservable<VehicleLiveData> VehicleData => VehicleDataSubject;

    public IObservable<VehicleFunctionData> VehicleFunctionData => VehicleFunctionDataSubject;

    public IObservableValue<bool> IsConnected => IsConnectedValue;

    public IObservableValue<TrackPower> TrackPower => TrackPowerValue;

    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas)
    {
        DriveCommands.AddRange(locoSetDriveDatas);
        return Task.CompletedTask;
    }

    public List<(ushort Address, ushort Function, bool On)> FunctionCommands { get; } = new();

    public Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on)
    {
        FunctionCommands.Add((vehicleAddress, functionIndex, on));
        return Task.CompletedTask;
    }

    public Task SetTrackPowerAsync(bool on)
    {
        TrackPowerCommands.Add(on);
        return Task.CompletedTask;
    }
}
