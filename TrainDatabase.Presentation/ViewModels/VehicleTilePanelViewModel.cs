using System.Collections.ObjectModel;
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

    [ObservableProperty] private string searchText = "";

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
