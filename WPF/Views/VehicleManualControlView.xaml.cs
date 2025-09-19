using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleManualControlView VehicleManualControlViewFactory(int vehicleId);

  public partial class VehicleManualControlView : UserControl
  {
    public VehicleManualControlView(int vehicleId, VehicleManualControlViewModelFactory vehicleManualControlViewModelFactory)
    {
      InitializeComponent();
      DataContext = vehicleManualControlViewModelFactory(vehicleId);
    }
  }
}