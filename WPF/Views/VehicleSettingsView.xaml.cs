using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleSettingsView VehicleSettingsViewFactory(int vehicleId);

  public partial class VehicleSettingsView : UserControl
  {
    public VehicleSettingsView(int vehicleId, VehicleSettingsViewModelFactory vehicleSettingsViewModelFactory)
    {
      InitializeComponent();
      DataContext = vehicleSettingsViewModelFactory(vehicleId);
    }
  }
}