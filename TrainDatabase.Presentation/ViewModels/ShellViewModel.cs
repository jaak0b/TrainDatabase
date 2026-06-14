using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Infrastructure;
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
    private readonly VehicleWorkspaceViewModel workspace;
    private readonly SettingsViewModel settings;
    private readonly DatabaseImportViewModel import;
    private readonly Lazy<MeasurementViewModel> measurement;

    [ObservableProperty] private ViewModelBase? current;
    [ObservableProperty] private DialogViewModel? currentDialog;
    [ObservableProperty] private bool isDisconnected;

    public ShellViewModel(
        NavigationService navigation,
        DialogService dialogService,
        VehicleTilePanelViewModel home,
        VehicleWorkspaceViewModel workspace,
        SettingsViewModel settings,
        DatabaseImportViewModel import,
        Lazy<MeasurementViewModel> measurement,
        IClientPresenter clientPresenter,
        IUiDispatcher dispatcher)
    {
        this.navigation = navigation;
        this.home = home;
        this.workspace = workspace;
        this.settings = settings;
        this.import = import;
        this.measurement = measurement;

        navigation.CurrentChanged += (_, _) => Current = navigation.Current;
        dialogService.CurrentChanged += (_, _) => CurrentDialog = dialogService.Current;
        clientPresenter.IsDisconnected.Subscribe(value => dispatcher.Post(() => IsDisconnected = value));

        navigation.NavigateTo(home);
    }

    [RelayCommand]
    private void GoHome() => navigation.NavigateTo(home);

    [RelayCommand]
    private void OpenWorkspace() => navigation.NavigateTo(workspace);

    [RelayCommand]
    private void OpenSettings() => navigation.NavigateTo(settings);

    [RelayCommand]
    private void OpenImport() => navigation.NavigateTo(import);

    [RelayCommand]
    private void OpenMeasurement() => navigation.NavigateTo(measurement.Value);
}
