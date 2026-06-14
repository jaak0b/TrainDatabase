using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Services;
using TrainDatabase.Presentation.Infrastructure;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleManualControlViewModel VehicleManualControlViewModelFactory(int vehicleId);

/// <summary>
/// Drives a single vehicle: speed slider + direction, reflecting live state from the
/// command station and pushing user changes back through the control service.
/// </summary>
public partial class VehicleManualControlViewModel : ViewModelBase
{
    private readonly IVehicleControlService control;
    private readonly IVehiclePresenter presenter;
    private bool suppressSend;

    [ObservableProperty] private int speed;
    [ObservableProperty] private int maximumSpeedStep;
    [ObservableProperty] private bool direction;
    [ObservableProperty] private bool isDisconnected;

    public VehicleManualControlViewModel(
        int vehicleId,
        VehiclePresenterFactory presenterFactory,
        IVehicleControlService control,
        IClientPresenter clientPresenter,
        IUiDispatcher dispatcher)
    {
        VehicleId = vehicleId;
        this.control = control;
        presenter = presenterFactory(vehicleId);

        Vehicle vehicle = presenter.Vehicle.Value;
        foreach (VehicleFunction function in vehicle.Functions.Where(f => f.IsActive).OrderBy(f => f.Position))
        {
            Functions.Add(new VehicleFunctionViewModel(vehicle, function, control));
        }

        presenter.Speed.Subscribe(value => dispatcher.Post(() => SetWithoutSending(() => Speed = value)));
        presenter.Direction.Subscribe(value => dispatcher.Post(() => SetWithoutSending(() => Direction = value)));
        presenter.MaximumSpeedStep.Subscribe(value => dispatcher.Post(() => MaximumSpeedStep = value));
        presenter.FunctionStates.Subscribe(states => dispatcher.Post(() => ApplyFunctionStates(states)));
        clientPresenter.IsDisconnected.Subscribe(value => dispatcher.Post(() => IsDisconnected = value));
    }

    public int VehicleId { get; }

    public ObservableCollection<VehicleFunctionViewModel> Functions { get; } = new();

    private void ApplyFunctionStates(IReadOnlyDictionary<ushort, bool> states)
    {
        foreach (VehicleFunctionViewModel function in Functions)
        {
            if (function.ButtonType == ButtonType.Switch && states.TryGetValue((ushort)function.FunctionIndex, out bool on))
            {
                function.SetLiveState(on);
            }
        }
    }

    partial void OnSpeedChanged(int value)
    {
        if (!suppressSend)
        {
            _ = SendDriveAsync();
        }
    }

    partial void OnDirectionChanged(bool value)
    {
        if (!suppressSend)
        {
            _ = SendDriveAsync();
        }
    }

    private Task SendDriveAsync() => control.SetVehicleSpeedAsync(presenter.Vehicle.Value, Speed, Direction);

    private void SetWithoutSending(Action set)
    {
        suppressSend = true;
        try
        {
            set();
        }
        finally
        {
            suppressSend = false;
        }
    }
}
