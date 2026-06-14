using CommunityToolkit.Mvvm.ComponentModel;
using TrainDatabase.Core.Domain;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>Editable view of a single vehicle function (the old EditFunctionWindow).</summary>
public partial class FunctionEditViewModel : ViewModelBase
{
    [ObservableProperty] private string name;
    [ObservableProperty] private int address;
    [ObservableProperty] private ButtonType buttonType;
    [ObservableProperty] private bool isActive;

    public FunctionEditViewModel(VehicleFunction function)
    {
        Id = function.Id;
        name = function.Name;
        address = function.Address;
        buttonType = function.ButtonType;
        isActive = function.IsActive;
    }

    public int Id { get; }

    public VehicleFunction ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        Address = Address,
        ButtonType = ButtonType,
        IsActive = IsActive,
    };
}
