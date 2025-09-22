using System.Net;
using System.Reactive.Subjects;
using Core;
using Core.Model;
using Reactive.Bindings;
using Z21;
using Z21.Events;
using Z21.Model;

namespace Adapter
{
  public class Z21ClientAdapter : IClientAdapter
  {
    private readonly Client client;
    private readonly Subject<VehicleLiveData> vehicleDataSubject = new();
    private readonly Subject<VehicleFunctionData> vehicleFunctionData = new();

    public Z21ClientAdapter(Client client)
    {
      this.client = client;
      client.ClientReachabilityChanged += Client_OnClientReachabilityChanged;
      client.OnGetLocoInfo += Client_OnOnGetLocoInfo;
      client.TrackPowerChanged += Client_OnTrackPowerChanged;
    }


    public void Connect(IPEndPoint endPoint)
    {
      client.Connect(endPoint.Address);
    }

    public IObservable<VehicleLiveData> VehicleData => vehicleDataSubject;

    public IObservable<VehicleFunctionData> VehicleFunctionData => vehicleFunctionData;

    public ReactiveProperty<bool> IsConnected { get; } = new();

    public ReactiveProperty<TrackPower> TrackPower { get; } = new();

    public async Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas)
    {
      client.SetLocoDrive(locoSetDriveDatas.Select(data => new LokInfoData(data.VehicleAddress) { DrivingDirection = data.Direction, Speed = data.Speed }).ToList());
    }

    public Task SetTrackPowerAsync(bool on)
    {
      if (on)
        client.SetTrackPowerON();
      else
      {
        client.SetTrackPowerOFF();
      }

      return Task.CompletedTask;
    }

    private void Client_OnOnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
    {
      vehicleDataSubject.OnNext(new()
                                {
                                  VehicleAddress = (ushort)e.Data.Adresse.Value,
                                  Speed = e.Data.Speed,
                                  Direction = e.Data.DrivingDirection
                                });

      vehicleFunctionData.OnNext(new()
                                 {
                                   VehicleAddress = (ushort)e.Data.Adresse.Value,
                                   FunctionState = e.Data.Functions.ToDictionary(tuple => (ushort)tuple.address, tuple => tuple.state)
                                 });
    }

    private void Client_OnClientReachabilityChanged(object? sender, bool isConnected)
    {
      IsConnected.Value = isConnected;
    }

    private void Client_OnTrackPowerChanged(object? sender, TrackPowerEventArgs e)
    {
      TrackPower.Value = e.TrackPower switch
                         {
                           Z21.Enums.TrackPower.OFF => Core.Model.TrackPower.Off,
                           Z21.Enums.TrackPower.ON => Core.Model.TrackPower.On,
                           Z21.Enums.TrackPower.Short => Core.Model.TrackPower.Short,
                           Z21.Enums.TrackPower.Programing => Core.Model.TrackPower.Programing,
                           _ => throw new ArgumentOutOfRangeException()
                         };
    }
  }
}