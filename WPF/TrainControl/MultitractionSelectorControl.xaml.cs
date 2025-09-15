using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Persistence.Database;
using Persistence.Entities;
using Shell.WPF.TimeCapture;

namespace Shell.WPF.TrainControl
{
  /// <summary>
  /// Interaction logic for Multitraction.xaml
  /// </summary>
  public partial class MultitractionSelectorControl : UserControl
  {
    public MultitractionSelectorControl(IServiceProvider serviceProvider, VehicleEntity vehicle)
    {
      InitializeComponent();
      ServiceProvider = serviceProvider;
      Vehicle = vehicle;
      Db = ServiceProvider.GetService<Database>()!;
      VehicleService = ServiceProvider.GetService<VehicleService>()!;

      Db.ChangeTracker.StateChanged += (a, b) => SearchTractionVehicles();

      SearchTractionVehicles();
    }

    private Database Db { get; }

    private IServiceProvider ServiceProvider { get; }

    private VehicleEntity Vehicle { get; }

    private VehicleService VehicleService { get; }

    private void DrawAllVehicles(IEnumerable<VehicleEntity> vehicles)
    {
      List<VehicleEntity> tractionVehicles = vehicles.Where(f => Vehicle.TractionVehicleIds.Any(e => e == f.Id))
                                                    .OrderBy(e => e.Position).ToList();
      if (tractionVehicles.Any())
      {
        AddListToStackPanel(tractionVehicles);
        SPVehilces.Children.Add(new Separator());
      }

      AddListToStackPanel(
                          vehicles.Where(f => !Vehicle.TractionVehicleIds.Any(e => e == f.Id))
                                  .OrderBy(e => e.Position));
    }

    private void AddListToStackPanel(IEnumerable<VehicleEntity> vehicles)
    {
      foreach (VehicleEntity vehicle in vehicles.Where(e => e.Id != Vehicle.Id))
      {
        CheckBox c = new()
                     {
                       Content = vehicle.Name,
                       Tag = vehicle,
                       Margin = new(5),
                       IsChecked = Vehicle.TractionVehicleIds.Any(e => e == vehicle.Id)
                     };
        c.Unchecked += (sender, e) =>
                       {
                         if ((sender as CheckBox)?.Tag is VehicleEntity vehicle)
                         {
                           VehicleService.RemoveTractionVehilce(vehicle, Vehicle);
                         }
                       };
        c.Checked += (sender, e) =>
                     {
                       if (sender is CheckBox c && c.Tag is VehicleEntity v)
                       {
                         VehicleService.AddTractionVehilce(v, Vehicle);
                       }
                     };
        SPVehilces.Children.Add(c);
      }
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
      SearchTractionVehicles();
    }

    public void SearchTractionVehicles()
    {
      SPVehilces.Children.Clear();
      if (!string.IsNullOrWhiteSpace(tbSearch.Text))
      {
        DrawAllVehicles(
                        Db.Vehicles.Where(
                                          i => (i.Address + i.Railway + i.Description + i.FullName + i.Name + i.Type)
                                              .ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
      }
      else
      {
        DrawAllVehicles(Db.Vehicles.OrderBy(e => e.Position));
      }
    }
  }
}