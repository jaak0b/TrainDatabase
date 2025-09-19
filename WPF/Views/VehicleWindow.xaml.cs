using System.Windows;

namespace Shell.WPF.Views
{
  public delegate VehicleWindow VehicleWindowFactory(int vehicleId);

  public partial class VehicleWindow : Window
  {
    public VehicleWindow(int vehicleId)
    {
      InitializeComponent();
    }
  }
}