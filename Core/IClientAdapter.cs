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

    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas);
  }
}