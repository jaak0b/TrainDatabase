using System;
using System.Reactive.Linq;
using Core;
using Core.Model;
using Core.Presenters;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Z21;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

  public class VehicleManualControlViewModel
  {
    private readonly IClientAdapter client;
    private readonly IVehiclePresenter vehiclePresenter;

    public VehicleManualControlViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehiclePresenterFactory presenterFactory, IClientAdapter client)
    {
      this.client = client;
      vehiclePresenter = presenterFactory(vehicleId);
      VehicleViewModel = vehicleViewModelFactory(vehicleId);

      VehicleSpeed = vehiclePresenter.Speed
                                     .Where(_ => !IsSliderDragged)
                                     .ToReactiveProperty();

      VehicleSpeed.Where(i => IsSliderDragged)
                  .Delay(new TimeSpan(500))
                  .Subscribe(i => client.SetVehiclesDriveAsync(new LocoSetDriveData() { VehicleAddress = (ushort)vehiclePresenter.Vehicle.Value.Address, Direction = true, Speed = (ushort)i }));
    }

    public VehicleViewModel VehicleViewModel { get; }

    public ReactiveProperty<int> VehicleSpeed { get; set; }

    public bool IsSliderDragged { get; set; }
  }
}