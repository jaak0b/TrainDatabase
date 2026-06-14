using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleDetailViewModel VehicleDetailViewModelFactory(int vehicleId);

/// <summary>
/// A control-only pane in the workspace: drives one vehicle and exposes its title and consist
/// size. Editing opens the full <see cref="VehicleEditViewModel"/> route; closing removes the
/// pane from the workspace.
/// </summary>
public partial class VehicleDetailViewModel : ViewModelBase
{
    private readonly VehicleWorkspaceViewModel workspace;
    private readonly VehicleEditViewModelFactory editFactory;
    private readonly INavigationService navigation;

    [ObservableProperty] private string title = "";
    [ObservableProperty] private byte[]? imageData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTraction))]
    [NotifyPropertyChangedFor(nameof(TractionLabel))]
    private int tractionCount;

    public VehicleDetailViewModel(
        int vehicleId,
        VehicleManualControlViewModelFactory controlFactory,
        VehiclePresenterFactory presenterFactory,
        VehicleWorkspaceViewModel workspace,
        VehicleEditViewModelFactory editFactory,
        INavigationService navigation,
        IVehicleImageStore imageStore,
        IUiDispatcher dispatcher)
    {
        this.workspace = workspace;
        this.editFactory = editFactory;
        this.navigation = navigation;
        VehicleId = vehicleId;
        Control = controlFactory(vehicleId);

        presenterFactory(vehicleId).Vehicle.Subscribe(vehicle => dispatcher.Post(() =>
        {
            Title = vehicle.Name;
            TractionCount = vehicle.TractionVehicleIds.Count;
            ImageData = imageStore.TryGetImage(vehicle.ImageName);
        }));
    }

    public int VehicleId { get; }

    public VehicleManualControlViewModel Control { get; }

    public bool HasTraction => TractionCount > 0;

    public string TractionLabel => $"{TractionCount + 1} locos";

    [RelayCommand]
    private void Edit() => navigation.NavigateTo(editFactory(VehicleId));

    [RelayCommand]
    private void Close() => workspace.ClosePane(this);
}
