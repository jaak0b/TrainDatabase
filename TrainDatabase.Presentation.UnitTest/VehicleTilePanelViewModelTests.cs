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
}
