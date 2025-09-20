using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleManualControlView VehicleManualControlViewFactory(int vehicleId);

  public partial class VehicleManualControlView : UserControl
  {
    private readonly VehicleManualControlViewModel vehicleManualControlViewModel;

    public VehicleManualControlView(int vehicleId, VehicleManualControlViewModelFactory vehicleManualControlViewModelFactory)
    {
      InitializeComponent();
      vehicleManualControlViewModel = vehicleManualControlViewModelFactory(vehicleId);
      DataContext = vehicleManualControlViewModel;
    }

    private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
    {
      vehicleManualControlViewModel.IsSliderDragged = true;
    }

    private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
      vehicleManualControlViewModel.IsSliderDragged = false;
    }

    private void VehicleManualControlView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      vehicleManualControlViewModel.SetVehicleSpeedFromScrollWheel(e.Delta);
      e.Handled = true;
    }
  }
}