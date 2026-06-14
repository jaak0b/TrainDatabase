using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Services;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Infrastructure;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>
/// Speed measurement (Einmessen): drives a selected vehicle through its speed steps while the
/// Arduino sensor records real speeds, storing the results as calibration data. Requires the
/// speed sensor connected and track power on.
/// </summary>
public partial class MeasurementViewModel : ViewModelBase
{
    private readonly IVehicleSpeedCalibrationService calibration;
    private readonly IVehicleRepository repository;
    private readonly IDialogService dialogs;

    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private VehicleListItem? selectedVehicle;
    [ObservableProperty] private string status = "Select a vehicle and start. Requires the speed sensor connected and track power on.";

    public MeasurementViewModel(
        IVehicleSpeedCalibrationService calibration,
        IVehicleRepository repository,
        IDialogService dialogs,
        IUiDispatcher dispatcher)
    {
        this.calibration = calibration;
        this.repository = repository;
        this.dialogs = dialogs;

        calibration.ServiceState.Subscribe(state => dispatcher.Post(() =>
        {
            IsRunning = state.IsRunning;
            StartCommand.NotifyCanExecuteChanged();
        }));

        Refresh();
    }

    public ObservableCollection<VehicleListItem> Vehicles { get; } = new();

    public void Refresh()
    {
        Vehicles.Clear();
        foreach (Vehicle vehicle in repository.FullTextSearchVehicles(null).OrderBy(v => v.Position))
        {
            Vehicles.Add(new VehicleListItem(vehicle.Id, vehicle.Name, vehicle.Address));
        }
    }

    partial void OnSelectedVehicleChanged(VehicleListItem? value) => StartCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        try
        {
            Status = "Measuring…";
            Vehicle vehicle = repository.GetVehicleByIdRequired(SelectedVehicle.Id);
            await calibration.CalibrateVehicleAsync(vehicle);
            await repository.UpdateVehicleAsync(vehicle);
            Status = "Measurement complete.";
            await dialogs.AlertAsync("Measurement", "Measurement complete.");
        }
        catch (Exception ex)
        {
            Status = "Measurement failed.";
            await dialogs.AlertAsync("Measurement failed", ex.Message);
        }
    }

    private bool CanStart() => !IsRunning && SelectedVehicle is not null;
}
