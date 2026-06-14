using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>The main vehicle grid with a live full-text search filter.</summary>
public partial class VehicleTilePanelViewModel : ViewModelBase
{
    private readonly IVehicleRepository repository;
    private readonly VehicleTileViewModelFactory tileFactory;
    private readonly INavigationService navigation;
    private readonly VehicleEditViewModelFactory editFactory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanReorder))]
    private string searchText = "";

    public VehicleTilePanelViewModel(
        IVehicleRepository repository,
        VehicleTileViewModelFactory tileFactory,
        INavigationService navigation,
        VehicleEditViewModelFactory editFactory)
    {
        this.repository = repository;
        this.tileFactory = tileFactory;
        this.navigation = navigation;
        this.editFactory = editFactory;
        Refresh();
    }

    public ObservableCollection<VehicleTileViewModel> Tiles { get; } = new();

    public bool CanReorder => string.IsNullOrEmpty(SearchText);

    public void MoveTile(int fromIndex, int toIndex) => Tiles.Move(fromIndex, toIndex);

    public void PersistOrder() =>
        repository.UpdateVehiclePositions(
            Tiles.Select((tile, index) => new VehiclePosition(tile.VehicleId, index)).ToList());

    [RelayCommand]
    private async Task Add()
    {
        int id = await repository.AddVehicleAsync(new Vehicle { Name = "New vehicle", Address = 3 });
        Refresh();
        navigation.NavigateTo(editFactory(id));
    }

    public void Refresh()
    {
        Tiles.Clear();
        foreach (Vehicle vehicle in repository.FullTextSearchVehicles(SearchText).OrderBy(v => v.Position))
        {
            Tiles.Add(tileFactory(vehicle.Id));
        }
    }

    partial void OnSearchTextChanged(string value) => Refresh();
}
