using CommandStation;
using CommandStation.Model;
using CommandStation.Transport;
using Z21.Core;
using Z21.Core.Command;

namespace TrainDatabase.Infrastructure.IntegrationTest.Fakes;

public sealed record DriveCall(ushort LocoAddress, DccSpeedMode SpeedMode, DrivingDirection Direction, ushort Speed);

public sealed record FunctionCall(ushort LocoAddress, ushort FunctionIndex, FunctionToggleType Toggle);

public sealed class FakeZ21CommandStation : IZ21CommandStation
{
    public List<DriveCall> Drives { get; } = new();
    public List<FunctionCall> Functions { get; } = new();
    public List<bool> TrackPowerCalls { get; } = new();
    public int ConnectCount { get; private set; }
    public int RequestStatusCount { get; private set; }
    public bool IsConnected { get; private set; }

    public event EventHandler<ConnectionChangedEventArgs>? ConnectionChanged;
    public event EventHandler<LocoInfoData>? LocoInfoReceived;
    public event EventHandler<bool>? TrackPowerChanged;
    public event EventHandler<CentralState>? StatusChanged;

    public void RaiseLocoInfo(LocoInfoData data) => LocoInfoReceived?.Invoke(this, data);

    public void RaiseTrackPower(bool on) => TrackPowerChanged?.Invoke(this, on);

    public void RaiseStatus(CentralState state) => StatusChanged?.Invoke(this, state);

    public void RaiseConnection(bool connected)
    {
        IsConnected = connected;
        ConnectionChanged?.Invoke(this, new ConnectionChangedEventArgs(connected));
    }

    public Task ConnectAsync()
    {
        ConnectCount++;
        RaiseConnection(true);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        RaiseConnection(false);
        return Task.CompletedTask;
    }

    public Task DriveAsync(ushort locoAddress, DccSpeedMode speedMode, DrivingDirection direction, ushort speed)
    {
        Drives.Add(new DriveCall(locoAddress, speedMode, direction, speed));
        return Task.CompletedTask;
    }

    public Task SetFunctionAsync(ushort locoAddress, ushort functionIndex, FunctionToggleType toggleType)
    {
        Functions.Add(new FunctionCall(locoAddress, functionIndex, toggleType));
        return Task.CompletedTask;
    }

    public Task TrackPowerOnAsync()
    {
        TrackPowerCalls.Add(true);
        return Task.CompletedTask;
    }

    public Task TrackPowerOffAsync()
    {
        TrackPowerCalls.Add(false);
        return Task.CompletedTask;
    }

    public Task RequestStatusAsync()
    {
        RequestStatusCount++;
        return Task.CompletedTask;
    }

    public IZ21CommandFactory Commands => throw new NotSupportedException();
    public event EventHandler<SystemState>? SystemStateReceived;
    public event EventHandler<FirmwareVersion>? FirmwareVersionReceived;
    public event EventHandler<TurnoutInfo>? TurnoutInfoReceived;
    public event EventHandler<ExtAccessoryInfo>? ExtAccessoryInfoReceived;
    public event EventHandler<CvValue>? CvReadCompleted;
    public event EventHandler<CvProgrammingError>? CvProgrammingFailed;
    public event EventHandler<FeedbackData>? FeedbackChanged;
    public event EventHandler<ModelTime>? ModelTimeChanged;

    public Task EmergencyStopAsync(ushort locoAddress) => throw new NotSupportedException();
    public Task PurgeAsync(ushort locoAddress) => throw new NotSupportedException();
    public Task RequestLocoInfoAsync(ushort locoAddress) => throw new NotSupportedException();
    public Task EmergencyStopAllAsync() => throw new NotSupportedException();
    public Task SetTurnoutAsync(ushort accessoryAddress, AccessoryOutput output, AccessoryState state, bool executeImmediately) => throw new NotSupportedException();
    public Task SetExtAccessoryAsync(ushort accessoryAddress, byte payload) => throw new NotSupportedException();
    public Task RequestTurnoutInfoAsync(ushort accessoryAddress) => throw new NotSupportedException();
    public Task RequestExtAccessoryInfoAsync(ushort accessoryAddress) => throw new NotSupportedException();
    public Task RequestSystemStateAsync() => throw new NotSupportedException();
    public Task RequestFirmwareVersionAsync() => throw new NotSupportedException();
    public Task ReadCvAsync(ushort cvAddress) => throw new NotSupportedException();
    public Task WriteCvAsync(ushort cvAddress, byte value) => throw new NotSupportedException();
    public Task<byte> ReadCvAsync(ushort cvAddress, TimeSpan timeout) => throw new NotSupportedException();
    public Task WriteCvAsync(ushort cvAddress, byte value, TimeSpan timeout) => throw new NotSupportedException();
    public Task RequestFeedbackAsync(byte groupIndex) => throw new NotSupportedException();
    public Task RequestModelTimeAsync() => throw new NotSupportedException();
    public Task SetModelTimeAsync(ModelTime time) => throw new NotSupportedException();
    public Task StartModelTimeAsync() => throw new NotSupportedException();
    public Task StopModelTimeAsync() => throw new NotSupportedException();
    public Task SendCommandsAsync(params IZ21Command[] commands) => throw new NotSupportedException();
    public Task<byte> ReadPomCvAsync(ushort locoAddress, ushort cvAddress, TimeSpan timeout) => throw new NotSupportedException();
    public Task WritePomCvAsync(ushort locoAddress, ushort cvAddress, byte value, TimeSpan timeout) => throw new NotSupportedException();
    public Task WritePomCvBitAsync(ushort locoAddress, ushort cvAddress, byte bitPosition, bool bitValue, TimeSpan timeout) => throw new NotSupportedException();
    public Task<bool> ReadPomCvBitAsync(ushort locoAddress, ushort cvAddress, byte bitPosition, TimeSpan timeout) => throw new NotSupportedException();
}
