using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Presenters;
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

    [ObservableProperty] private string title = "";

    public VehicleEditViewModel(
        int vehicleId,
        VehicleSettingsViewModelFactory settingsFactory,
        VehiclePresenterFactory presenterFactory,
        INavigationService navigation,
        IUiDispatcher dispatcher)
    {
        this.navigation = navigation;
        VehicleId = vehicleId;
        Settings = settingsFactory(vehicleId);
        presenterFactory(vehicleId).Vehicle.Subscribe(vehicle => dispatcher.Post(() => Title = vehicle.Name));
    }

    public int VehicleId { get; }

    public VehicleSettingsViewModel Settings { get; }

    [RelayCommand]
    private void Back() => navigation.Back();
}
