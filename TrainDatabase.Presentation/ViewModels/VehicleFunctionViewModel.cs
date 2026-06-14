using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Services;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>
/// A single vehicle function (F-button). Behaviour depends on the configured
/// <see cref="ButtonType"/>: Switch toggles, PushButton pulses, Timer turns on for a duration.
/// </summary>
public partial class VehicleFunctionViewModel : ViewModelBase
{
    private readonly Vehicle vehicle;
    private readonly VehicleFunction function;
    private readonly IVehicleControlService control;

    [ObservableProperty] private bool isActive;

    public VehicleFunctionViewModel(Vehicle vehicle, VehicleFunction function, IVehicleControlService control)
    {
        this.vehicle = vehicle;
        this.function = function;
        this.control = control;
    }

    public int FunctionIndex => function.Address;

    public string Label => string.IsNullOrWhiteSpace(function.Name) ? $"F{function.Address}" : function.Name;

    public ButtonType ButtonType => function.ButtonType;

    /// <summary>Reflects a live state update from the command station (for Switch buttons).</summary>
    public void SetLiveState(bool on) => IsActive = on;

    [RelayCommand]
    private async Task Activate()
    {
        switch (function.ButtonType)
        {
            case ButtonType.Switch:
                IsActive = !IsActive;
                await control.SetVehicleFunctionAsync(vehicle, function.Address, IsActive);
                break;

            case ButtonType.PushButton:
                await control.SetVehicleFunctionAsync(vehicle, function.Address, true);
                await Task.Delay(250);
                await control.SetVehicleFunctionAsync(vehicle, function.Address, false);
                break;

            case ButtonType.Timer:
                IsActive = true;
                await control.SetVehicleFunctionAsync(vehicle, function.Address, true);
                await Task.Delay(Math.Max(0, function.Time) * 1000);
                await control.SetVehicleFunctionAsync(vehicle, function.Address, false);
                IsActive = false;
                break;
        }
    }
}
