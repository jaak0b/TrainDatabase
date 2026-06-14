using Avalonia.Controls;
using Avalonia.Headless.NUnit;
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

public class WorkspaceE2ETests
{
    [AvaloniaTest]
    public void OpeningTwoTrains_ShowsTwoPanes_AndCloseRemovesOne()
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
            context.Vehicles.Add(new VehicleEntity { Name = "Loco A", Address = 11 });
            context.Vehicles.Add(new VehicleEntity { Name = "Loco B", Address = 12 });
            context.SaveChanges();

            ShellViewModel shell = services.GetRequiredService<ShellViewModel>();
            MainWindow window = new() { DataContext = shell };
            window.Show();

            VehicleTilePanelViewModel panel = (VehicleTilePanelViewModel)shell.Current!;
            panel.Refresh();
            panel.Tiles[0].OpenCommand.Execute(null);
            panel.Tiles[1].OpenCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            VehicleWorkspaceViewModel workspace = (VehicleWorkspaceViewModel)shell.Current!;
            Assert.That(workspace.Panes, Has.Count.EqualTo(2));

            int renderedPanes = window.GetVisualDescendants().OfType<VehicleDetailView>().Count();
            Assert.That(renderedPanes, Is.EqualTo(2));

            workspace.Panes[0].CloseCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.That(workspace.Panes, Has.Count.EqualTo(1));
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

    [AvaloniaTest]
    public void TileEditButton_NavigatesToEditScreen()
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
            context.Vehicles.Add(new VehicleEntity { Name = "Loco A", Address = 11 });
            context.SaveChanges();

            ShellViewModel shell = services.GetRequiredService<ShellViewModel>();
            MainWindow window = new() { DataContext = shell };
            window.Show();

            VehicleTilePanelViewModel panel = (VehicleTilePanelViewModel)shell.Current!;
            panel.Refresh();
            panel.Tiles[0].EditCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.That(shell.Current, Is.InstanceOf<VehicleEditViewModel>());
            Assert.That(window.GetVisualDescendants().OfType<VehicleEditView>().Any(), Is.True);
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
