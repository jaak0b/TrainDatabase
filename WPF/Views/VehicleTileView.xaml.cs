using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleTileView VehicleTileViewFactory(ushort vehicleAddress);
  
  public partial class VehicleTileView : UserControl
  {
    public VehicleTileView(VehicleViewModel vehicleViewModel)
    {
      InitializeComponent();
    }
  }
}