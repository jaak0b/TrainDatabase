using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleTilePanelViewModelTests
{
    [Test]
    public async Task Add_CreatesVehicle_RefreshesTiles_AndOpensEditScreen()
    {
        using TestContainer test = new();
        VehicleTilePanelViewModel home = test.Resolve<VehicleTilePanelViewModel>();
        NavigationService navigation = test.Resolve<NavigationService>();

        await home.AddCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(home.Tiles, Has.Count.EqualTo(1));
            Assert.That(navigation.Current, Is.InstanceOf<VehicleEditViewModel>());
        });
    }

    [Test]
    public void MoveTile_ReordersTilesInPlace()
    {
        using TestContainer test = new(
            new Vehicle { Id = 1, Name = "A", Position = 0 },
            new Vehicle { Id = 2, Name = "B", Position = 1 },
            new Vehicle { Id = 3, Name = "C", Position = 2 });
        VehicleTilePanelViewModel home = test.Resolve<VehicleTilePanelViewModel>();

        home.MoveTile(0, 2);

        Assert.That(home.Tiles.Select(tile => tile.VehicleId), Is.EqualTo(new[] { 2, 3, 1 }));
    }

    [Test]
    public void PersistOrder_WritesSequentialPositions()
    {
        using TestContainer test = new(
            new Vehicle { Id = 1, Name = "A", Position = 0 },
            new Vehicle { Id = 2, Name = "B", Position = 1 });
        VehicleTilePanelViewModel home = test.Resolve<VehicleTilePanelViewModel>();

        home.MoveTile(0, 1);
        home.PersistOrder();

        Assert.That(test.Repository.PositionUpdates,
            Is.EqualTo(new[] { new VehiclePosition(2, 0), new VehiclePosition(1, 1) }));
    }

    [Test]
    public void CanReorder_FalseWhileSearching()
    {
        using TestContainer test = new(new Vehicle { Id = 1, Name = "A" });
        VehicleTilePanelViewModel home = test.Resolve<VehicleTilePanelViewModel>();

        Assert.That(home.CanReorder, Is.True);

        home.SearchText = "br";

        Assert.That(home.CanReorder, Is.False);
    }
}
