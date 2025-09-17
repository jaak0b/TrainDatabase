using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleTileView VehicleTileViewFactory(int vehicleId);

  public partial class VehicleTileView : UserControl
  {
    public VehicleTileView(VehicleViewModelFactory vehicleViewModelFactory, int vehicleId)
    {
      InitializeComponent();
      VehicleViewModel = vehicleViewModelFactory(vehicleId);
      DataContext = VehicleViewModel;
    }

    public VehicleViewModel VehicleViewModel { get; }


  }
}