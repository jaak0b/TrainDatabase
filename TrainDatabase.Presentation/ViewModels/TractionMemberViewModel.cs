using CommunityToolkit.Mvvm.ComponentModel;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>A candidate vehicle that can be coupled into another vehicle's multi-traction consist.</summary>
public partial class TractionMemberViewModel : ViewModelBase
{
    [ObservableProperty] private bool isSelected;

    public TractionMemberViewModel(int vehicleId, string name, bool isSelected)
    {
        VehicleId = vehicleId;
        Name = name;
        this.isSelected = isSelected;
    }

    public int VehicleId { get; }

    public string Name { get; }
}
