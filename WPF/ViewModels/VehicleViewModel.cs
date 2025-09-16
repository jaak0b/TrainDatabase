using System.ComponentModel;
using System.Runtime.CompilerServices;
using Persistence.Model;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleViewModel VehicleViewModelFactory(ushort vehicleAddress);
  
  public class VehicleViewModel(Vehicle vehicle) : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}