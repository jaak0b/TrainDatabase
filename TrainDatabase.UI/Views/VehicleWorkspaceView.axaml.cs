using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.UI.Views;

public partial class VehicleWorkspaceView : UserControl
{
    public VehicleWorkspaceView()
    {
        InitializeComponent();
        AddTrainButton.Click += OnAddTrainClick;
    }

    private void OnAddTrainClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not VehicleWorkspaceViewModel workspace)
        {
            return;
        }

        ListBox list = new()
        {
            ItemsSource = workspace.AvailableVehicles,
            MaxHeight = 320,
            ItemTemplate = new FuncDataTemplate<Vehicle>((_, _) =>
                new TextBlock { [!TextBlock.TextProperty] = new Binding(nameof(Vehicle.Name)) }, true),
        };

        Flyout flyout = new() { Content = list };
        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is Vehicle vehicle)
            {
                workspace.OpenVehicle(vehicle.Id);
                flyout.Hide();
            }
        };

        flyout.ShowAt(AddTrainButton);
    }
}
