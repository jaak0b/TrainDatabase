using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Persistence.Ports;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Shell.WPF.Views;

namespace Shell.WPF.ViewModels
{
  public class VehicleTilePanelViewModel : IDropTarget
  {
    private readonly IVehicleRepository vehicleRepository;
    private readonly VehicleTileViewFactory vehicleTileViewFactory;

    public VehicleTilePanelViewModel(IVehicleRepository vehicleRepository, VehicleTileViewFactory vehicleTileViewFactory)
    {
      this.vehicleRepository = vehicleRepository;
      this.vehicleTileViewFactory = vehicleTileViewFactory;

      RefreshTiles();
      VehicleSearchText.Throttle(TimeSpan.FromMilliseconds(500)).DistinctUntilChanged().ObserveOnUIDispatcher().Subscribe(_ => RefreshTiles());
    }

    public ObservableCollection<VehicleTileView> VehicleViews { get; } = [];

    public ReactiveProperty<string> VehicleSearchText { get; set; } = new();

    private void RefreshTiles()
    {
      VehicleViews.Clear();
      foreach (VehicleTileView vehicleTileView in vehicleRepository.FullTextSearchVehicles(VehicleSearchText.Value)
                                                                   .OrderBy(vehicle => vehicle.Position)
                                                                   .Select(vehicle => vehicleTileViewFactory(vehicle.Id)))
      {
        VehicleViews.Add(vehicleTileView);
      }
    }

    public void DragOver(IDropInfo dropInfo)
    {
      if (dropInfo.Data is VehicleTileView && Equals(dropInfo.TargetCollection, VehicleViews))
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    public void Drop(IDropInfo dropInfo)
    {
      if (dropInfo.Data is VehicleTileView sourceItem && Equals(dropInfo.TargetCollection, VehicleViews))
      {
        int insertIndex = dropInfo.InsertIndex;
        int sourceIndex = VehicleViews.IndexOf(sourceItem);

        if (sourceIndex != -1 && insertIndex != sourceIndex)
        {
          VehicleViews.Move(sourceIndex, insertIndex);
        }
      }
    }
  }
}