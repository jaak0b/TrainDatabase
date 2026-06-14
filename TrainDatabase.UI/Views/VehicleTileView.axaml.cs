using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.UI.Views;

public partial class VehicleTileView : UserControl
{
    public static readonly DataFormat<VehicleTileViewModel> DragFormat =
        DataFormat.CreateInProcessFormat<VehicleTileViewModel>("traindatabase/vehicle-tile");

    private const double DragThreshold = 6;

    private Point pressOrigin;
    private PointerPressedEventArgs? pressArgs;

    public VehicleTileView() => InitializeComponent();

    private void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || IsWithinEditButton(e.Source))
        {
            pressArgs = null;
            return;
        }

        pressArgs = e;
        pressOrigin = e.GetPosition(this);
    }

    private async void OnCardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (pressArgs is not { } trigger || DataContext is not VehicleTileViewModel tile)
        {
            return;
        }

        Point current = e.GetPosition(this);
        if (Math.Abs(current.X - pressOrigin.X) < DragThreshold && Math.Abs(current.Y - pressOrigin.Y) < DragThreshold)
        {
            return;
        }

        pressArgs = null;
        DataTransfer data = new();
        data.Add(DataTransferItem.Create(DragFormat, tile));
        try
        {
            await DragDrop.DoDragDropAsync(trigger, data, DragDropEffects.Move);
        }
        finally
        {
            this.FindAncestorOfType<VehicleTilePanelView>()?.CommitOrder();
        }
    }

    private void OnCardKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            (DataContext as VehicleTileViewModel)?.OpenCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnCardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (pressArgs is null)
        {
            return;
        }

        pressArgs = null;
        if (!IsWithinEditButton(e.Source))
        {
            (DataContext as VehicleTileViewModel)?.OpenCommand.Execute(null);
        }
    }

    private bool IsWithinEditButton(object? source) =>
        source is Visual visual && visual.GetSelfAndVisualAncestors().Any(ancestor => ReferenceEquals(ancestor, EditButton));
}
