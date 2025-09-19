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
    private readonly VehicleTileViewModelFactory vehicleTileViewModelFactory;

    public VehicleTilePanelViewModel(IVehicleRepository vehicleRepository, VehicleTileViewModelFactory vehicleTileViewModelFactory)
    {
      this.vehicleRepository = vehicleRepository;
      this.vehicleTileViewModelFactory = vehicleTileViewModelFactory;

      RefreshTiles();
      VehicleSearchText.Throttle(TimeSpan.FromMilliseconds(500)).DistinctUntilChanged().ObserveOnUIDispatcher().Subscribe(_ => RefreshTiles());
    }

    public ObservableCollection<VehicleTileViewModel> VehicleTiles { get; } = [];

    public ReactiveProperty<string> VehicleSearchText { get; set; } = new();

    private void RefreshTiles()
    {
      VehicleTiles.Clear();
      foreach (VehicleTileViewModel vehicleTileView in vehicleRepository.FullTextSearchVehicles(VehicleSearchText.Value)
                                                                        .OrderBy(vehicle => vehicle.Position)
                                                                        .Select(vehicle => vehicleTileViewModelFactory(vehicle.Id)))
      {
        VehicleTiles.Add(vehicleTileView);
      }
    }

    public void DragOver(IDropInfo dropInfo)
    {
      if (dropInfo.Data is VehicleTileViewModel && Equals(dropInfo.TargetCollection, VehicleTiles))
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    public void Drop(IDropInfo dropInfo)
    {
      if (dropInfo.Data is not VehicleTileViewModel sourceItem || !Equals(dropInfo.TargetCollection, VehicleTiles))
        return;

      int sourceIndex = VehicleTiles.IndexOf(sourceItem);
      int insertIndex = dropInfo.InsertIndex;

      insertIndex = Math.Max(0, Math.Min(insertIndex, VehicleTiles.Count - 1));

      if (insertIndex >= VehicleTiles.Count || insertIndex == sourceIndex)
        return;

      VehicleTiles.Move(sourceIndex, insertIndex);

      var changedPositions = VehicleTiles
                            .Select((vehicleTileViewModel, index) => new
                                                                     {
                                                                       viewModel = vehicleTileViewModel,
                                                                       vehicleId = vehicleTileViewModel.VehicleViewModel.Vehicle.Value.Id,
                                                                       newPosition = index,
                                                                       oldPosition = vehicleTileViewModel.VehicleViewModel.Vehicle.Value.Position
                                                                     })
                            .Where(arg => arg.newPosition != arg.oldPosition)
                            .ToList();

      changedPositions.ForEach(obj => obj.viewModel.VehicleViewModel.Vehicle.Value.Position = obj.newPosition);

      vehicleRepository.UpdateVehiclePositions(changedPositions.Select(arg => (arg.vehicleId, arg.newPosition)));
    }
  }
}