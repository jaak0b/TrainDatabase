using System.Net;
using System.Reactive.Subjects;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;
using Z21;
using Z21.Events;
using Z21.Model;
using CoreTrackPower = TrainDatabase.Core.Live.TrackPower;

namespace TrainDatabase.Infrastructure.Hardware;

/// <summary>
/// Implements <see cref="IClientAdapter"/> over the z21 UDP <see cref="Client"/>, translating
/// its events into reactive streams and current-value observables.
/// </summary>
public sealed class Z21ClientAdapter : IClientAdapter
{
    private readonly Client client;
    private readonly Subject<VehicleLiveData> vehicleData = new();
    private readonly Subject<VehicleFunctionData> vehicleFunctionData = new();
    private readonly ObservableValue<bool> isConnected = new(false);
    private readonly ObservableValue<CoreTrackPower> trackPower = new(CoreTrackPower.Off);

    public Z21ClientAdapter(Client client)
    {
        this.client = client;
        client.ClientReachabilityChanged += OnClientReachabilityChanged;
        client.OnGetLocoInfo += OnGetLocoInfo;
        client.TrackPowerChanged += OnTrackPowerChanged;
    }

    public void Connect(IPEndPoint endPoint) => client.Connect(endPoint.Address);

    public IObservable<VehicleLiveData> VehicleData => vehicleData;

    public IObservable<VehicleFunctionData> VehicleFunctionData => vehicleFunctionData;

    public IObservableValue<bool> IsConnected => isConnected;

    public IObservableValue<CoreTrackPower> TrackPower => trackPower;

    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas)
    {
        client.SetLocoDrive(locoSetDriveDatas
            .Select(data => new LokInfoData((int)data.VehicleAddress)
            {
                DrivingDirection = data.Direction,
                Speed = data.Speed,
            })
            .ToList());
        return Task.CompletedTask;
    }

    public Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on)
    {
        client.SetLocoFunction(new FunctionData(
            new LokAdresse(vehicleAddress),
            functionIndex,
            on ? Z21.Enums.ToggleType.On : Z21.Enums.ToggleType.Off));
        return Task.CompletedTask;
    }

    public Task SetTrackPowerAsync(bool on)
    {
        if (on)
        {
            client.SetTrackPowerON();
        }
        else
        {
            client.SetTrackPowerOFF();
        }

        return Task.CompletedTask;
    }

    private void OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
    {
        ushort address = (ushort)e.Data.Adresse.Value;
        vehicleData.OnNext(new VehicleLiveData
        {
            VehicleAddress = address,
            Speed = e.Data.Speed,
            Direction = e.Data.DrivingDirection,
        });

        vehicleFunctionData.OnNext(new VehicleFunctionData
        {
            VehicleAddress = address,
            FunctionState = e.Data.Functions.ToDictionary(tuple => (ushort)tuple.address, tuple => tuple.state),
        });
    }

    private void OnClientReachabilityChanged(object? sender, bool reachable) => isConnected.SetValue(reachable);

    private void OnTrackPowerChanged(object? sender, TrackPowerEventArgs e) =>
        trackPower.SetValue(e.TrackPower switch
        {
            Z21.Enums.TrackPower.OFF => CoreTrackPower.Off,
            Z21.Enums.TrackPower.ON => CoreTrackPower.On,
            Z21.Enums.TrackPower.Short => CoreTrackPower.Short,
            Z21.Enums.TrackPower.Programing => CoreTrackPower.Programing,
            _ => CoreTrackPower.Off,
        });
}
