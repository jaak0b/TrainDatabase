using System.Windows;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleWindow VehicleWindowFactory(int vehicleId);

  public partial class VehicleWindow : Window
  {
    public VehicleWindow(int vehicleId, VehicleWindowViewModelFactory vehicleWindowViewModelFactory)
    {
      InitializeComponent();
      DataContext = vehicleWindowViewModelFactory(vehicleId);
    }
  }
}