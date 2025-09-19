using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleTileView VehicleTileViewFactory(int vehicleId);

  public partial class VehicleTileView : UserControl
  {
    public VehicleTileView(VehicleTileViewModelFactory vehicleViewModelFactory, int vehicleId)
    {
      InitializeComponent();
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
      DataContext = VehicleViewModel;
    }

    public VehicleTileViewModel VehicleViewModel { get; }
  }
}