using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using TrainDatabase.Presentation.ViewModels;
using TrainDatabase.UI.Converters;

namespace TrainDatabase.UI.Views;

public partial class VehicleTilePanelView : UserControl
{
    public VehicleTilePanelView()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (DataContext is not VehicleTilePanelViewModel panel
            || e.DataTransfer?.TryGetValue(VehicleTileView.DragFormat) is not VehicleTileViewModel dragged
            || !panel.CanReorder)
        {
            e.DragEffects = DragDropEffects.None;
            Ghost.IsVisible = false;
            return;
        }

        e.DragEffects = DragDropEffects.Move;
        UpdateGhost(dragged, e.GetPosition(GhostLayer));

        int from = panel.Tiles.IndexOf(dragged);
        int to = TargetIndex(e);
        if (from >= 0 && to >= 0 && from != to)
        {
            panel.MoveTile(from, to);
        }
    }

    private void OnDrop(object? sender, DragEventArgs e) => Ghost.IsVisible = false;

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (!Bounds.Contains(e.GetPosition(this)))
        {
            Ghost.IsVisible = false;
        }
    }

    public void CommitOrder()
    {
        Ghost.IsVisible = false;
        if (DataContext is VehicleTilePanelViewModel panel && panel.CanReorder)
        {
            panel.PersistOrder();
        }
    }

    private int TargetIndex(DragEventArgs e)
    {
        if (DataContext is not VehicleTilePanelViewModel panel)
        {
            return -1;
        }

        foreach (VehicleTileView view in TilesHost.GetVisualDescendants().OfType<VehicleTileView>())
        {
            Point local = e.GetPosition(view);
            if (local.X >= 0 && local.Y >= 0 && local.X <= view.Bounds.Width && local.Y <= view.Bounds.Height
                && view.DataContext is VehicleTileViewModel target)
            {
                return panel.Tiles.IndexOf(target);
            }
        }

        return -1;
    }

    private void UpdateGhost(VehicleTileViewModel dragged, Point position)
    {
        if (!ReferenceEquals(Ghost.Tag, dragged))
        {
            Ghost.Tag = dragged;
            GhostName.Text = dragged.Name;
            GhostImage.Source = BytesToBitmapConverter.Instance.Convert(
                dragged.ImageData, typeof(IImage), null, CultureInfo.InvariantCulture) as IImage;
        }

        Canvas.SetLeft(Ghost, position.X + 12);
        Canvas.SetTop(Ghost, position.Y + 12);
        Ghost.IsVisible = true;
    }
}
