using System.Net;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.Ports;

/// <summary>
/// Abstraction over a digital command station (today: Roco/Fleischmann z21 over UDP).
/// Implemented per platform in the Infrastructure layer; the Browser head supplies a
/// no-op <c>NullClientAdapter</c>.
/// </summary>
public interface IClientAdapter
{
    /// <summary>Begins connecting to the command station at the given endpoint.</summary>
    void Connect(IPEndPoint endPoint);

    /// <summary>Live speed/direction updates for vehicles.</summary>
    IObservable<VehicleLiveData> VehicleData { get; }

    /// <summary>Live function-state updates for vehicles.</summary>
    IObservable<VehicleFunctionData> VehicleFunctionData { get; }

    /// <summary>Whether the command station is currently reachable.</summary>
    IObservableValue<bool> IsConnected { get; }

    /// <summary>Current track power state.</summary>
    IObservableValue<TrackPower> TrackPower { get; }

    /// <summary>Sets drive (speed + direction) for one or more vehicles.</summary>
    Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas);

    /// <summary>Turns a vehicle function (F0..Fn) on or off.</summary>
    Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on);

    /// <summary>Turns track power on or off.</summary>
    Task SetTrackPowerAsync(bool on);
}
