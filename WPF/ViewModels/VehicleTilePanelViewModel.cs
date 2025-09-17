using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using MugenMvvmToolkit;
using Persistence.Ports;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public class VehicleTilePanelViewModel
  {
    private readonly IVehicleRepository vehicleRepository;
    private readonly VehicleTileViewFactory vehicleTileViewFactory;

    public VehicleTilePanelViewModel(IVehicleRepository vehicleRepository, VehicleTileViewFactory vehicleTileViewFactory)
    {
      this.vehicleRepository = vehicleRepository;
      this.vehicleTileViewFactory = vehicleTileViewFactory;

      RefreshTiles();
      VehicleSearchText.Throttle(TimeSpan.FromMilliseconds(200)).DistinctUntilChanged().ObserveOnUIDispatcher().Subscribe(_ => RefreshTiles());
    }

    public ObservableCollection<VehicleTileView> VehicleViews { get; set; } = [];

    public ReactiveProperty<string> VehicleSearchText { get; set; } = new();

    private void RefreshTiles()
    {
      VehicleViews.Clear();

      VehicleViews.AddRange(vehicleRepository.FullTextSearchVehicles(VehicleSearchText.Value)
                                             .Select(vehicle => vehicleTileViewFactory(vehicle.Id)));
    }
  }
}