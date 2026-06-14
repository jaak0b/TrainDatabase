using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>A short summary of a vehicle for the management list.</summary>
public sealed record VehicleListItem(int Id, string Name, long Address)
{
    public string Display => $"#{Address} — {(string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name)}";
}

/// <summary>Manage the roster: add, delete, and open a vehicle for editing.</summary>
public partial class VehicleManagementViewModel : ViewModelBase
{
    private readonly IVehicleRepository repository;
    private readonly IDialogService dialogs;
    private readonly INavigationService navigation;
    private readonly VehicleDetailViewModelFactory detailFactory;

    public VehicleManagementViewModel(
        IVehicleRepository repository,
        IDialogService dialogs,
        INavigationService navigation,
        VehicleDetailViewModelFactory detailFactory)
    {
        this.repository = repository;
        this.dialogs = dialogs;
        this.navigation = navigation;
        this.detailFactory = detailFactory;
        Refresh();
    }

    public ObservableCollection<VehicleListItem> Vehicles { get; } = new();

    public void Refresh()
    {
        Vehicles.Clear();
        foreach (Vehicle vehicle in repository.FullTextSearchVehicles(null).OrderBy(v => v.Position))
        {
            Vehicles.Add(new VehicleListItem(vehicle.Id, vehicle.Name, vehicle.Address));
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        int id = await repository.AddVehicleAsync(new Vehicle { Name = "New vehicle", Address = 3 });
        Refresh();
        navigation.NavigateTo(detailFactory(id));
    }

    [RelayCommand]
    private void Edit(VehicleListItem? item)
    {
        if (item is not null)
        {
            navigation.NavigateTo(detailFactory(item.Id));
        }
    }

    [RelayCommand]
    private async Task Delete(VehicleListItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (await dialogs.ConfirmAsync("Delete vehicle", $"Delete '{item.Display}'? This cannot be undone."))
        {
            await repository.DeleteVehicleAsync(item.Id);
            Refresh();
        }
    }
}
