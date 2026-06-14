using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>The main vehicle grid with a live full-text search filter.</summary>
public partial class VehicleTilePanelViewModel : ViewModelBase
{
    private readonly IVehicleRepository repository;
    private readonly VehicleTileViewModelFactory tileFactory;

    [ObservableProperty] private string searchText = "";

    public VehicleTilePanelViewModel(IVehicleRepository repository, VehicleTileViewModelFactory tileFactory)
    {
        this.repository = repository;
        this.tileFactory = tileFactory;
        Refresh();
    }

    public ObservableCollection<VehicleTileViewModel> Tiles { get; } = new();

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
