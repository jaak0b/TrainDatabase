using System;
using System.Reactive.Linq;
using Core;
using Core.Model;
using Core.Presenters;
using Core.Services;
using Reactive.Bindings;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

  public class VehicleManualControlViewModel
  {

    public VehicleManualControlViewModel(int vehicleId, VehicleViewModelFactory vehicleViewModelFactory, VehiclePresenterFactory presenterFactory, IVehicleControlService vehicleControlService)
    {
      VehiclePresenter = presenterFactory(vehicleId);
      VehicleViewModel = vehicleViewModelFactory(vehicleId);

      VehicleSpeed = VehiclePresenter.Speed
                                     .Where(_ => !IsSpeedChangeUserInitiated)
                                     .ToReactiveProperty();

      VehicleDirection = VehiclePresenter.Direction
                                         .Where(_ => !IsDirectionChangedUserInitiated)
                                         .ToReactiveProperty();

      VehicleSpeed.Where(_ => IsSpeedChangeUserInitiated)
                  .Sample(TimeSpan.FromMilliseconds(200))
                  .Subscribe(async void (speed) => await vehicleControlService.SetVehicleSpeedAsync(VehicleViewModel.Vehicle.Value, speed, VehicleDirection.Value));

      VehicleDirection.Where(_ => IsDirectionChangedUserInitiated)
                      .Subscribe(async void (direction) =>
                                 {
                                   IsDirectionChangedUserInitiated = false;
                                   await vehicleControlService.SetVehicleSpeedAsync(VehicleViewModel.Vehicle.Value, VehicleSpeed.Value, direction);
                                 });

      VehicleDirection.Subscribe(vehicleDirection => VehicleDirectionDisplayText.Value
                                                       = vehicleDirection ? Resources.Resources.VehicleDirectionBackwards : Resources.Resources.VehicleDirectionForward);
    }

    public IVehiclePresenter VehiclePresenter { get; }

    public VehicleViewModel VehicleViewModel { get; }

    public ReactiveProperty<int> VehicleSpeed { get; set; }

    public ReactiveProperty<bool> VehicleDirection { get; set; }

    public ReactiveProperty<string> VehicleDirectionDisplayText { get; set; } = new();

    public bool IsSpeedChangeUserInitiated { get; set; }

    public bool IsDirectionChangedUserInitiated { get; set; }

    public void SetVehicleSpeedFromScrollWheel(int delta)
    {
      try
      {
        IsSpeedChangeUserInitiated = true;
        VehicleSpeed.Value = delta < 0 ? VehicleSpeed.Value - 1 : VehicleSpeed.Value + 1;
      } finally
      {
        IsSpeedChangeUserInitiated = false;
      }
    }
  }
}