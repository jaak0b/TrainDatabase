using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>
/// Root of the single-page app: hosts the active route (<see cref="Current"/>) and the
/// active overlay dialog (<see cref="CurrentDialog"/>), plus top-level navigation commands.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly NavigationService navigation;
    private readonly VehicleTilePanelViewModel home;
    private readonly SettingsViewModel settings;
    private readonly DatabaseImportViewModel import;
    private readonly VehicleManagementViewModel management;
    private readonly Lazy<MeasurementViewModel> measurement;

    [ObservableProperty] private ViewModelBase? current;
    [ObservableProperty] private DialogViewModel? currentDialog;

    public ShellViewModel(
        NavigationService navigation,
        DialogService dialogService,
        VehicleTilePanelViewModel home,
        SettingsViewModel settings,
        DatabaseImportViewModel import,
        VehicleManagementViewModel management,
        Lazy<MeasurementViewModel> measurement)
    {
        this.navigation = navigation;
        this.home = home;
        this.settings = settings;
        this.import = import;
        this.management = management;
        this.measurement = measurement;

        navigation.CurrentChanged += (_, _) => Current = navigation.Current;
        dialogService.CurrentChanged += (_, _) => CurrentDialog = dialogService.Current;

        navigation.NavigateTo(home);
    }

    [RelayCommand]
    private void GoHome() => navigation.NavigateTo(home);

    [RelayCommand]
    private void OpenSettings() => navigation.NavigateTo(settings);

    [RelayCommand]
    private void OpenImport() => navigation.NavigateTo(import);

    [RelayCommand]
    private void OpenManagement() => navigation.NavigateTo(management);

    [RelayCommand]
    private void OpenMeasurement() => navigation.NavigateTo(measurement.Value);
}
