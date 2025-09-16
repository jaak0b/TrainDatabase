using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Factories;
using Core.Presenters;
using Persistence.Model;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleViewModel VehicleViewModelFactory(ushort vehicleAddress);
  
  public class VehicleViewModel(VehiclePresenterFactory presenterFactory, ushort vehicleAddress) : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}