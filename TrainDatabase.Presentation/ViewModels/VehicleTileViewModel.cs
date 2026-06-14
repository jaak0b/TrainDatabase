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

    [ObservableProperty] private string name = "";
    [ObservableProperty] private int speed;
    [ObservableProperty] private byte[]? imageData;

    public VehicleTileViewModel(
        int vehicleId,
        VehiclePresenterFactory presenterFactory,
        IVehicleImageStore imageStore,
        IUiDispatcher dispatcher,
        INavigationService navigation,
        VehicleWorkspaceViewModel workspace)
    {
        VehicleId = vehicleId;
        this.navigation = navigation;
        this.workspace = workspace;

        IVehiclePresenter presenter = presenterFactory(vehicleId);
        Name = presenter.Vehicle.Value.Name;
        ImageData = imageStore.TryGetImage(presenter.Vehicle.Value.ImageName);
        presenter.Vehicle.Subscribe(vehicle => dispatcher.Post(() =>
        {
            Name = vehicle.Name;
            ImageData = imageStore.TryGetImage(vehicle.ImageName);
        }));
        presenter.Speed.Subscribe(value => dispatcher.Post(() => Speed = value));
    }

    public int VehicleId { get; }

    [RelayCommand]
    private void Open()
    {
        workspace.OpenVehicle(VehicleId);
        navigation.NavigateTo(workspace);
    }
}
