using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleDetailViewModel VehicleDetailViewModelFactory(int vehicleId);

/// <summary>
/// Hosts the per-vehicle control and settings (the WPF VehicleWindow tabs) as a single route.
/// </summary>
public partial class VehicleDetailViewModel : ViewModelBase
{
    private readonly INavigationService navigation;

    public VehicleDetailViewModel(
        int vehicleId,
        VehicleManualControlViewModelFactory controlFactory,
        VehicleSettingsViewModelFactory settingsFactory,
        INavigationService navigation)
    {
        this.navigation = navigation;
        VehicleId = vehicleId;
        Control = controlFactory(vehicleId);
        Settings = settingsFactory(vehicleId);
    }

    public int VehicleId { get; }

    public VehicleManualControlViewModel Control { get; }

    public VehicleSettingsViewModel Settings { get; }

    public bool CanGoBack => navigation.CanGoBack;

    [RelayCommand]
    private void Back() => navigation.Back();
}
