using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Presentation.ViewModels;

public delegate VehicleSettingsViewModel VehicleSettingsViewModelFactory(int vehicleId);

/// <summary>Edits a vehicle's basic settings and its functions, persisting through the repository.</summary>
public partial class VehicleSettingsViewModel : ViewModelBase
{
    private readonly IVehicleRepository repository;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private string fullName = "";
    [ObservableProperty] private long address;
    [ObservableProperty] private string railway = "";
    [ObservableProperty] private bool isActive;
    [ObservableProperty] private VehicleType type;
    [ObservableProperty] private RegulationStep regulationStep;
    [ObservableProperty] private bool invertTraction;
    [ObservableProperty] private string description = "";

    public VehicleSettingsViewModel(int vehicleId, IVehicleRepository repository)
    {
        this.repository = repository;
        VehicleId = vehicleId;
        Load(repository.GetVehicleByIdRequired(vehicleId));
    }

    public int VehicleId { get; }

    public ObservableCollection<FunctionEditViewModel> Functions { get; } = new();

    public ObservableCollection<TractionMemberViewModel> Members { get; } = new();

    public static IReadOnlyList<ButtonType> ButtonTypes { get; } = Enum.GetValues<ButtonType>();

    public static IReadOnlyList<VehicleType> VehicleTypes { get; } = Enum.GetValues<VehicleType>();

    public static IReadOnlyList<RegulationStep> RegulationSteps { get; } = Enum.GetValues<RegulationStep>();

    private void Load(Vehicle vehicle)
    {
        Name = vehicle.Name;
        FullName = vehicle.FullName;
        Address = vehicle.Address;
        Railway = vehicle.Railway;
        IsActive = vehicle.IsActive;
        Type = vehicle.Type;
        RegulationStep = vehicle.RegulationStep;
        InvertTraction = vehicle.InvertTraction;
        Description = vehicle.Description;

        Functions.Clear();
        foreach (VehicleFunction function in vehicle.Functions.OrderBy(f => f.Position))
        {
            Functions.Add(new FunctionEditViewModel(function));
        }

        Members.Clear();
        foreach (Vehicle candidate in repository.FullTextSearchVehicles("").Where(v => v.Id != VehicleId).OrderBy(v => v.Position))
        {
            Members.Add(new TractionMemberViewModel(candidate.Id, candidate.Name, vehicle.TractionVehicleIds.Contains(candidate.Id)));
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        Vehicle vehicle = repository.GetVehicleByIdRequired(VehicleId);
        vehicle.Name = Name;
        vehicle.FullName = FullName;
        vehicle.Address = Address;
        vehicle.Railway = Railway;
        vehicle.IsActive = IsActive;
        vehicle.Type = Type;
        vehicle.RegulationStep = RegulationStep;
        vehicle.InvertTraction = InvertTraction;
        vehicle.Description = Description;
        vehicle.TractionVehicleIds = Members.Where(m => m.IsSelected).Select(m => m.VehicleId).ToList();
        await repository.UpdateVehicleAsync(vehicle);
        await repository.UpdateVehicleFunctionsAsync(VehicleId, Functions.Select(f => f.ToDomain()).ToList());
    }

    [RelayCommand]
    private void Revert() => Load(repository.GetVehicleByIdRequired(VehicleId));
}
