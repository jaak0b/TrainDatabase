using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public class MainWindowViewModel(VehicleTilePanelView vehicleTilePanelView)
  {
    public VehicleTilePanelView VehicleTilePanelView { get; } = vehicleTilePanelView;
  }
}