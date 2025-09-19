using System.Net;
using System.Reactive.Subjects;
using Core;
using Core.Model;
using Reactive.Bindings;
using Z21;
using Z21.Events;

namespace ClientAdapter
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
    }

    public void Connect(IPEndPoint endPoint)
    {
      client.Connect(endPoint.Address);
    }

    public IObservable<VehicleLiveData> VehicleData => vehicleDataSubject;

    public IObservable<VehicleFunctionData> VehicleFunctionData => vehicleFunctionData;

    public ReactiveProperty<bool> IsConnected { get; } = new();

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
  }
}