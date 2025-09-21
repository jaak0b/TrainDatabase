using System.Windows;
using System.Windows.Controls;
using Shell.WPF.ViewModels;

namespace Shell.WPF.Views
{
  public delegate VehicleSettingsView VehicleSettingsViewFactory(int vehicleId);

  public partial class VehicleSettingsView : UserControl
  {
    private readonly VehicleSettingsViewModel vehicleSettingsViewModel;

    public VehicleSettingsView(int vehicleId, VehicleSettingsViewModelFactory vehicleSettingsViewModelFactory)
    {
      InitializeComponent();
      vehicleSettingsViewModel = vehicleSettingsViewModelFactory(vehicleId);
      DataContext = vehicleSettingsViewModel;
    }

    private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
      await vehicleSettingsViewModel.SaveChangesAsync();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
      vehicleSettingsViewModel.RevertChanges();
    }
  }
}