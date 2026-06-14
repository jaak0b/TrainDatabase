using System.Net;
using System.Reactive.Subjects;
using CommandStation;
using CommandStation.Model;
using CommandStation.Transport;
using CommandStation.Transport.Udp;
using Microsoft.Extensions.Logging;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;
using Z21.Core;
using CoreTrackPower = TrainDatabase.Core.Live.TrackPower;

namespace TrainDatabase.Infrastructure.Hardware;

public sealed class Z21ClientAdapter : IClientAdapter
{
    private readonly IZ21CommandStation station;
    private readonly UdpTransportOptions transportOptions;
    private readonly ILogger<Z21ClientAdapter> logger;
    private readonly Subject<VehicleLiveData> vehicleData = new();
    private readonly Subject<VehicleFunctionData> vehicleFunctionData = new();
    private readonly ObservableValue<bool> isConnected = new(false);
    private readonly ObservableValue<CoreTrackPower> trackPower = new(CoreTrackPower.Off);

    public Z21ClientAdapter(IZ21CommandStation station, UdpTransportOptions transportOptions, ILogger<Z21ClientAdapter> logger)
    {
        this.station = station;
        this.transportOptions = transportOptions;
        this.logger = logger;
        station.ConnectionChanged += OnConnectionChanged;
        station.LocoInfoReceived += OnLocoInfoReceived;
        station.TrackPowerChanged += OnTrackPowerChanged;
        station.StatusChanged += OnStatusChanged;
    }

    public IObservable<VehicleLiveData> VehicleData => vehicleData;

    public IObservable<VehicleFunctionData> VehicleFunctionData => vehicleFunctionData;

    public IObservableValue<bool> IsConnected => isConnected;

    public IObservableValue<CoreTrackPower> TrackPower => trackPower;

    public void Connect(IPEndPoint endPoint)
    {
        transportOptions.RemoteEndPoint = endPoint;
        _ = ConnectCoreAsync();
    }

    public async Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas)
    {
        if (!isConnected.Value)
        {
            return;
        }

        foreach (LocoSetDriveData data in locoSetDriveDatas)
        {
            DccSpeedMode mode = ToDccSpeedMode(data.SpeedStep);
            ushort maximum = MaxSpeed(mode);
            await station.DriveAsync(
                data.VehicleAddress,
                mode,
                data.Direction ? DrivingDirection.Forward : DrivingDirection.Backward,
                data.Speed > maximum ? maximum : data.Speed);
        }
    }

    public Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on)
    {
        if (!isConnected.Value)
        {
            return Task.CompletedTask;
        }

        return station.SetFunctionAsync(vehicleAddress, functionIndex, on ? FunctionToggleType.On : FunctionToggleType.Off);
    }

    public Task SetTrackPowerAsync(bool on)
    {
        if (!isConnected.Value)
        {
            return Task.CompletedTask;
        }

        return on ? station.TrackPowerOnAsync() : station.TrackPowerOffAsync();
    }

    private async Task ConnectCoreAsync()
    {
        try
        {
            await station.ConnectAsync();
            await station.RequestStatusAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to connect to the command station at {Endpoint}.", transportOptions.RemoteEndPoint);
        }
    }

    private static DccSpeedMode ToDccSpeedMode(RegulationStep step) => step switch
    {
        RegulationStep.Step14 => DccSpeedMode.Steps14,
        RegulationStep.Step28 => DccSpeedMode.Steps28,
        _ => DccSpeedMode.Steps128,
    };

    private static ushort MaxSpeed(DccSpeedMode mode) => mode switch
    {
        DccSpeedMode.Steps14 => 14,
        DccSpeedMode.Steps28 => 28,
        _ => 126,
    };

    private void OnLocoInfoReceived(object? sender, LocoInfoData info)
    {
        vehicleData.OnNext(new VehicleLiveData
        {
            VehicleAddress = info.LocoAddress,
            Speed = info.LocoSpeed,
            Direction = info.DrivingDirection == DrivingDirection.Forward,
        });

        vehicleFunctionData.OnNext(new VehicleFunctionData
        {
            VehicleAddress = info.LocoAddress,
            FunctionState = info.LocoFunctionsData.ToDictionary(
                function => (ushort)function.FunctionIndex,
                function => function.FunctionToggleType == FunctionToggleType.On),
        });
    }

    private void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e) => isConnected.SetValue(e.IsConnected);

    private void OnTrackPowerChanged(object? sender, bool on)
    {
        if (trackPower.Value is CoreTrackPower.Short or CoreTrackPower.Programing)
        {
            return;
        }

        trackPower.SetValue(on ? CoreTrackPower.On : CoreTrackPower.Off);
    }

    private void OnStatusChanged(object? sender, CentralState state) =>
        trackPower.SetValue(state switch
        {
            { ShortCircuit: true } => CoreTrackPower.Short,
            { ProgrammingModeActive: true } => CoreTrackPower.Programing,
            { TrackVoltageOff: true } => CoreTrackPower.Off,
            _ => CoreTrackPower.On,
        });
}
