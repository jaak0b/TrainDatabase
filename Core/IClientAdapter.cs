using System;
using System.Net;
using System.Threading.Tasks;
using Core.Model;
using Reactive.Bindings;

namespace Core
{
  public interface IClientAdapter
  {
    public void Connect(IPEndPoint endPoint);

    IObservable<VehicleLiveData> VehicleData { get; }

    IObservable<VehicleFunctionData> VehicleFunctionData { get; }

    ReactiveProperty<bool> IsConnected { get; }
    
    public ReactiveProperty<TrackPower> TrackPower { get; }

    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas);

    /// <summary>
    /// Sets the track power to a given state.
    /// </summary>
    /// <param name="on">True sets the track power to on. False sets the track power off</param>
    public Task SetTrackPowerAsync(bool on);
  }
}