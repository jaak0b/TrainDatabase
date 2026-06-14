using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleEditViewModel VehicleEditViewModelFactory(int vehicleId);

/// <summary>
/// Full-screen editor for a single vehicle (details, functions and multi-traction), reached
/// from a control pane's edit button. Back returns to the workspace.
/// </summary>
public partial class VehicleEditViewModel : ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly IVehicleRepository repository;
    private readonly IDialogService dialogs;
    private readonly VehicleWorkspaceViewModel workspace;

    [ObservableProperty] private string title = "";

    public VehicleEditViewModel(
        int vehicleId,
        VehicleSettingsViewModelFactory settingsFactory,
        VehiclePresenterFactory presenterFactory,
        INavigationService navigation,
        IVehicleRepository repository,
        IDialogService dialogs,
        VehicleWorkspaceViewModel workspace,
        IUiDispatcher dispatcher)
    {
        this.navigation = navigation;
        this.repository = repository;
        this.dialogs = dialogs;
        this.workspace = workspace;
        VehicleId = vehicleId;
        Settings = settingsFactory(vehicleId);
        presenterFactory(vehicleId).Vehicle.Subscribe(vehicle => dispatcher.Post(() => Title = vehicle.Name));
    }

    public int VehicleId { get; }

    public VehicleSettingsViewModel Settings { get; }

    [RelayCommand]
    private void Back() => navigation.Back();

    [RelayCommand]
    private async Task Delete()
    {
        if (!await dialogs.ConfirmAsync("Delete vehicle", $"Delete '{Title}'? This cannot be undone."))
        {
            return;
        }

        VehicleDetailViewModel? openPane = workspace.Panes.FirstOrDefault(pane => pane.VehicleId == VehicleId);
        if (openPane is not null)
        {
            workspace.ClosePane(openPane);
        }

        await repository.DeleteVehicleAsync(VehicleId);
        navigation.Back();
    }
}
