using System;
using System.Reactive.Linq;
using Core;
using Core.Model;
using Core.Presenters;
using Reactive.Bindings;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

  public class VehicleManualControlViewModel
  {
    private readonly IClientAdapter client;

    public VehicleManualControlViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehiclePresenterFactory presenterFactory, IClientAdapter client)
    {
      this.client = client;
      VehiclePresenter = presenterFactory(vehicleId);
      VehicleViewModel = vehicleViewModelFactory(vehicleId);

      VehicleSpeed = VehiclePresenter.Speed
                                     .Where(_ => !IsSliderDragged)
                                     .ToReactiveProperty();

      VehicleSpeed.Where(i => IsSliderDragged)
                  .Sample(TimeSpan.FromMilliseconds(200))
                  .Subscribe(i => client.SetVehiclesDriveAsync(new LocoSetDriveData() { VehicleAddress = (ushort)VehiclePresenter.Vehicle.Value.Address, Direction = true, Speed = (ushort)i }));
    }

    public  IVehiclePresenter VehiclePresenter { get; }
    
    public VehicleViewModel VehicleViewModel { get; }

    public ReactiveProperty<int> VehicleSpeed { get; set; }

    public bool IsSliderDragged { get; set; }

    public void SetVehicleSpeedFromScrollWheel(int delta)
    {
      try
      {
        IsSliderDragged = true;
        VehicleSpeed.Value = delta < 0 ? VehicleSpeed.Value - 1 : VehicleSpeed.Value + 1;
      } finally
      {
        IsSliderDragged = false;
      }
    }
  }
}