using Avalonia.Headless.NUnit;
using Microsoft.Extensions.DependencyInjection;
using TrainDatabase.Composition;
using TrainDatabase.Core.Live;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Presentation;
using TrainDatabase.Presentation.ViewModels;
using TrainDatabase.UI;
using TrainDatabase.UI.Views;

namespace TrainDatabase.UI.EndToEndTest;

public class ShellE2ETests
{
    [AvaloniaTest]
    public void App_ShowsVehicleList_NavigatesToDetail_AndDrivesLoco()
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

            // Seed a vehicle into the (temp) database.
            TrainDbContext context = services.GetRequiredService<TrainDbContext>();
            context.Vehicles.Add(new VehicleEntity { Name = "E2E Loco", Address = 11 });
            context.SaveChanges();

            ShellViewModel shell = services.GetRequiredService<ShellViewModel>();
            MainWindow window = new() { DataContext = shell };
            window.Show();

            // Home route shows the vehicle panel.
            VehicleTilePanelViewModel panel = (VehicleTilePanelViewModel)shell.Current!;
            panel.Refresh();
            Assert.That(panel.Tiles, Has.Count.EqualTo(1));
            Assert.That(panel.Tiles[0].Name, Is.EqualTo("E2E Loco"));

            // Open the vehicle -> detail route.
            panel.Tiles[0].OpenCommand.Execute(null);
            Assert.That(shell.Current, Is.InstanceOf<VehicleDetailViewModel>());

            // Drive the loco via the manual control -> reaches the (fake) command station.
            VehicleDetailViewModel detail = (VehicleDetailViewModel)shell.Current!;
            detail.Control.Speed = 50;

            Assert.That(client.DriveCommands, Has.Some.Matches<LocoSetDriveData>(c => c.VehicleAddress == 11 && c.Speed == 50));

            // Back returns to the vehicle list.
            detail.BackCommand.Execute(null);
            Assert.That(shell.Current, Is.InstanceOf<VehicleTilePanelViewModel>());
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
                // Best-effort temp cleanup; a lingering file handle must not fail the test.
            }
        }
    }
}
