using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>
/// The control workspace: hosts an open, control-only pane per vehicle the user is driving.
/// Opening a vehicle that is already open activates its pane instead of duplicating it.
/// </summary>
public partial class VehicleWorkspaceViewModel : ViewModelBase
{
    private readonly VehicleDetailViewModelFactory paneFactory;
    private readonly IVehicleRepository repository;

    [ObservableProperty] private VehicleDetailViewModel? activePane;

    public VehicleWorkspaceViewModel(VehicleDetailViewModelFactory paneFactory, IVehicleRepository repository)
    {
        this.paneFactory = paneFactory;
        this.repository = repository;
    }

    public ObservableCollection<VehicleDetailViewModel> Panes { get; } = new();

    public IReadOnlyList<Vehicle> AvailableVehicles =>
        repository.FullTextSearchVehicles("")
            .Where(vehicle => Panes.All(pane => pane.VehicleId != vehicle.Id))
            .OrderBy(vehicle => vehicle.Position)
            .ToList();

    public void OpenVehicle(int vehicleId)
    {
        VehicleDetailViewModel? existing = Panes.FirstOrDefault(p => p.VehicleId == vehicleId);
        if (existing is not null)
        {
            ActivePane = existing;
            return;
        }

        VehicleDetailViewModel pane = paneFactory(vehicleId);
        Panes.Add(pane);
        ActivePane = pane;
    }

    public void ClosePane(VehicleDetailViewModel pane)
    {
        Panes.Remove(pane);
        if (ReferenceEquals(ActivePane, pane))
        {
            ActivePane = Panes.LastOrDefault();
        }
    }

    [RelayCommand]
    private void OpenVehicleById(int vehicleId) => OpenVehicle(vehicleId);
}
