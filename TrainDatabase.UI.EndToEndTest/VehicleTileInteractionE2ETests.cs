using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using TrainDatabase.Composition;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Presentation;
using TrainDatabase.Presentation.ViewModels;
using TrainDatabase.UI.Views;

namespace TrainDatabase.UI.EndToEndTest;

public class VehicleTileInteractionE2ETests
{
    private static Point Center(Visual visual, Visual root) =>
        visual.TranslatePoint(new Point(visual.Bounds.Width / 2, visual.Bounds.Height / 2), root) ?? new Point();

    [AvaloniaTest]
    public void ClickingTileBody_OpensVehicleInWorkspace()
    {
        Run((shell, window) =>
        {
            VehicleTileView tile = window.GetVisualDescendants().OfType<VehicleTileView>().First();
            Point center = Center(tile, window);

            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.That(shell.Current, Is.InstanceOf<VehicleWorkspaceViewModel>());
        });
    }

    [AvaloniaTest]
    public void DraggingTileOntoAnother_ReordersTiles()
    {
        Run((shell, window) =>
        {
            VehicleTilePanelViewModel panel = (VehicleTilePanelViewModel)shell.Current!;
            List<VehicleTileView> tiles = window.GetVisualDescendants().OfType<VehicleTileView>().ToList();
            VehicleTileViewModel dragged = (VehicleTileViewModel)tiles[0].DataContext!;

            DataTransfer data = new();
            data.Add(DataTransferItem.Create(VehicleTileView.DragFormat, dragged));
            Point target = Center(tiles[1], window);

            window.DragDrop(target, RawDragEventType.DragEnter, data, DragDropEffects.Move);
            window.DragDrop(target, RawDragEventType.DragOver, data, DragDropEffects.Move);
            window.DragDrop(target, RawDragEventType.Drop, data, DragDropEffects.Move);
            Dispatcher.UIThread.RunJobs();

            Assert.That(panel.Tiles[1], Is.SameAs(dragged));
        });
    }

    [AvaloniaTest]
    public void PressingEnterOnFocusedTile_OpensVehicle()
    {
        Run((shell, window) =>
        {
            VehicleTileView tile = window.GetVisualDescendants().OfType<VehicleTileView>().First();
            Border card = tile.GetVisualDescendants().OfType<Border>().First();
            card.Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.That(shell.Current, Is.InstanceOf<VehicleWorkspaceViewModel>());
        });
    }

    [AvaloniaTest]
    public void DragLeaveOntoChildTile_KeepsGhostVisible()
    {
        Run((shell, window) =>
        {
            List<VehicleTileView> tiles = window.GetVisualDescendants().OfType<VehicleTileView>().ToList();
            DataTransfer data = new();
            data.Add(DataTransferItem.Create(VehicleTileView.DragFormat, (VehicleTileViewModel)tiles[0].DataContext!));
            Point inside = Center(tiles[1], window);
            Border ghost = window.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "Ghost");

            window.DragDrop(inside, RawDragEventType.DragEnter, data, DragDropEffects.Move);
            window.DragDrop(inside, RawDragEventType.DragOver, data, DragDropEffects.Move);
            Dispatcher.UIThread.RunJobs();
            Assert.That(ghost.IsVisible, Is.True, "ghost should show during the drag");

            window.DragDrop(inside, RawDragEventType.DragLeave, data, DragDropEffects.Move);
            Dispatcher.UIThread.RunJobs();
            Assert.That(ghost.IsVisible, Is.True, "ghost should stay visible when the pointer is still inside the panel");
        });
    }

    [AvaloniaTest]
    public void EditButton_HiddenUntilHover()
    {
        Run((shell, window) =>
        {
            VehicleTileView tile = window.GetVisualDescendants().OfType<VehicleTileView>().First();
            Button edit = tile.GetVisualDescendants().OfType<Button>().First(button => button.Name == "EditButton");

            Assert.That(edit.IsVisible, Is.False, "edit button should be hidden when the card is not hovered");

            window.MouseMove(Center(tile, window));
            Dispatcher.UIThread.RunJobs();

            Assert.That(edit.IsVisible, Is.True, "edit button should appear while the card is hovered");
        });
    }

    private static void Run(Action<ShellViewModel, Window> body)
    {
        string baseDirectory = Path.Combine(Path.GetTempPath(), "TrainDatabase.E2E", Guid.NewGuid().ToString("N"));
        FakeClientAdapter client = new();

        IServiceProvider services = Bootstrapper.InitializeAsync(
            loggerFactory: null,
            new PresentationModule(),
            new UiModule(),
            new TestOverrideModule(baseDirectory, client)).GetAwaiter().GetResult();

        try
        {
            App.Services = services;

            TrainDbContext context = services.GetRequiredService<TrainDbContext>();
            context.Vehicles.Add(new VehicleEntity { Name = "Loco A", Address = 11, Position = 0 });
            context.Vehicles.Add(new VehicleEntity { Name = "Loco B", Address = 12, Position = 1 });
            context.SaveChanges();

            ShellViewModel shell = services.GetRequiredService<ShellViewModel>();
            Window window = new MainWindow { DataContext = shell };
            window.Show();

            ((VehicleTilePanelViewModel)shell.Current!).Refresh();
            Dispatcher.UIThread.RunJobs();

            body(shell, window);
        }
        finally
        {
            (services as IDisposable)?.Dispose();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try
            {
                if (Directory.Exists(baseDirectory))
                {
                    Directory.Delete(baseDirectory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
        }
    }
}
