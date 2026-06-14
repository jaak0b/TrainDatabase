using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleTileViewModel VehicleTileViewModelFactory(int vehicleId);

/// <summary>A vehicle tile in the main grid: image + name + live speed, opens the detail route.</summary>
public partial class VehicleTileViewModel : ViewModelBase
{
    private readonly INavigationService navigation;
    private readonly VehicleWorkspaceViewModel workspace;
    private readonly VehicleEditViewModelFactory editFactory;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private byte[]? imageData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedDisplay))]
    private int speed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedDisplay))]
    private bool direction;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedDisplay))]
    private bool isConnected;

    public VehicleTileViewModel(
        int vehicleId,
        VehiclePresenterFactory presenterFactory,
        IVehicleImageStore imageStore,
        IClientPresenter clientPresenter,
        IUiDispatcher dispatcher,
        INavigationService navigation,
        VehicleWorkspaceViewModel workspace,
        VehicleEditViewModelFactory editFactory)
    {
        VehicleId = vehicleId;
        this.navigation = navigation;
        this.workspace = workspace;
        this.editFactory = editFactory;

        IVehiclePresenter presenter = presenterFactory(vehicleId);
        Name = presenter.Vehicle.Value.Name;
        ImageData = imageStore.TryGetImage(presenter.Vehicle.Value.ImageName);
        presenter.Vehicle.Subscribe(vehicle => dispatcher.Post(() =>
        {
            Name = vehicle.Name;
            ImageData = imageStore.TryGetImage(vehicle.ImageName);
        }));
        presenter.Speed.Subscribe(value => dispatcher.Post(() => Speed = value));
        presenter.Direction.Subscribe(value => dispatcher.Post(() => Direction = value));
        clientPresenter.IsConnected.Subscribe(value => dispatcher.Post(() => IsConnected = value));
    }

    public int VehicleId { get; }

    public string SpeedDisplay
    {
        get
        {
            string speedText = $"{(IsConnected ? Speed.ToString() : "-")} SS";
            return Direction ? $"< {speedText}  " : $"  {speedText} >";
        }
    }

    [RelayCommand]
    private void Open()
    {
        workspace.OpenVehicle(VehicleId);
        navigation.NavigateTo(workspace);
    }

    [RelayCommand]
    private void Edit() => navigation.NavigateTo(editFactory(VehicleId));
}
