using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using Core.Controller;
using Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Database;
using Persistence.Entities;
using Persistence.Enums;
using Shell.WPF.TimeCapture;

namespace Shell.WPF
{
  public class ComPortsComboBox : ComboBox
  {
    public ComPortsComboBox()
    {
      SetupManagementEventWatcher();
      UpdateComPortList();
      SelectionChanged += (a, b) => Configuration.ArduinoComPort = SelectedComPort;
    }

    public static List<string> ComPorts => ArduinoSerialPort.GetPortNames().ToList();

    public string SelectedComPort => $"{SelectedItem}";

    private ManagementEventWatcher ManagementEventWatcher { get; } = new();

    private void ManagementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
    {
      UpdateComPortList();
    }

    private void SetupManagementEventWatcher()
    {
      ManagementEventWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
      ManagementEventWatcher.EventArrived += new(ManagementEventWatcher_EventArrived);
      ManagementEventWatcher.Start();
    }

    private void UpdateComPortList()
    {
      Dispatcher.Invoke(() =>
                        {
                          object? selectedItem = SelectedItem ?? Configuration.ArduinoComPort;
                          Items.Clear();
                          ComPorts.ForEach(e => Items.Add(e));
                          if (selectedItem is not null)
                          {
                            SelectedItem = selectedItem;
                          }
                        });
    }
  }

  /// <summary>
  /// Interaction logic for Einmessen.xaml
  /// </summary>
  public partial class Einmessen : Window, INotifyPropertyChanged
  {
    private VehicleEntity? _vehicle = default!;

    public Einmessen(IServiceProvider serviceProvider)
    {
      DataContext = this;
      ServiceProvider = serviceProvider;
      Db = ServiceProvider.GetService<Database>()!;

      InitializeComponent();

      TiMultitraction.Content = new MultiTraction(ServiceProvider, Vehicle);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IServiceProvider ServiceProvider { get; }

    public Database Db { get; } = default!;

    private VehicleEntity? Vehicle
    {
      get => _vehicle;
      set
      {
        _vehicle = value;
        OnPropertyChanged();
      }
    }

    protected void OnPropertyChanged()
    {
      PropertyChanged?.Invoke(this, new(null));
    }

    private void BtnStopSpeedMeasurement_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private async void CmbAllVehicles_Loaded(object sender, RoutedEventArgs e)
    {
      CmbAllVehicles.Items.Clear();
      foreach (VehicleEntity vehicle in await Db.Vehicles.Where(e => e.Type == VehicleType.Lokomotive).OrderBy(e => e.Address).ToListAsync())
      {
        CmbAllVehicles.Items.Add(new ComboBoxItem
                                 {
                                   Tag = vehicle,
                                   Content = $"({vehicle.Address:D3})  {(string.IsNullOrWhiteSpace(vehicle.Name) ? vehicle.FullName : vehicle.Name)}"
                                 });
      }
    }

    private void CmbAllVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      TcMeasure.IsEnabled = CmbAllVehicles.SelectedItem is not VehicleEntity;
      if (CmbAllVehicles.SelectedItem is not ComboBoxItem cbi || cbi.Tag is not VehicleEntity vehicle)
      {
        return;
      }

      Vehicle = vehicle;
      TiMultitraction.Content = new MultiTraction(ServiceProvider, Vehicle);
    }
  }
}