using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public partial class VehicleTilePanelView : UserControl
  {
    public VehicleTilePanelView(VehicleTilePanelViewModel vehicleTilePanelViewModel)
    {
      InitializeComponent();
      DataContext = vehicleTilePanelViewModel;
    }
  }
}