using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleTileViewModelTests
{
    [Test]
    public void ImageData_SeededFromImageStore()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco", ImageName = "loco.png" });
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        Assert.That(tile.ImageData, Is.EqualTo(FakeVehicleImageStore.SampleImage));
    }

    [Test]
    public void Edit_NavigatesToEditRouteForSameVehicle()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco" });
        NavigationService navigation = test.Resolve<NavigationService>();
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        tile.EditCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(navigation.Current, Is.InstanceOf<VehicleEditViewModel>());
            Assert.That(((VehicleEditViewModel)navigation.Current!).VehicleId, Is.EqualTo(5));
        });
    }

    [Test]
    public void Open_OpensVehicleInWorkspace()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco" });
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        NavigationService navigation = test.Resolve<NavigationService>();
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        tile.OpenCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(workspace.Panes.Any(pane => pane.VehicleId == 5), Is.True);
            Assert.That(navigation.Current, Is.InstanceOf<VehicleWorkspaceViewModel>());
        });
    }

    [Test]
    public void NameAndImage_UpdateOnVehiclePush()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Old", ImageName = "" });
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);
        Assert.That(tile.ImageData, Is.Null);

        test.Presenter(5).VehicleValue.SetValue(new Vehicle { Id = 5, Name = "New", ImageName = "loco.png" });

        Assert.Multiple(() =>
        {
            Assert.That(tile.Name, Is.EqualTo("New"));
            Assert.That(tile.ImageData, Is.EqualTo(FakeVehicleImageStore.SampleImage));
        });
    }

    [Test]
    public void SpeedDisplay_ShowsStepsAndDirectionArrow_AndUpdatesOnPush()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco" });
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        Assert.That(tile.SpeedDisplay, Is.EqualTo("  0 SS >"));

        test.Presenter(5).DirectionValue.SetValue(true);

        Assert.That(tile.SpeedDisplay, Is.EqualTo("< 0 SS  "));
    }

    [Test]
    public void SpeedDisplay_UpdatesOnSpeedPush()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco" });
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        test.Presenter(5).SpeedValue.SetValue(42);

        Assert.That(tile.SpeedDisplay, Is.EqualTo("  42 SS >"));
    }

    [Test]
    public void SpeedDisplay_ShowsDash_WhenDisconnected()
    {
        using TestContainer test = new(new Vehicle { Id = 5, Name = "Loco" });
        VehicleTileViewModel tile = test.Resolve<VehicleTileViewModelFactory>()(5);

        ((FakeClientPresenter)test.Resolve<IClientPresenter>()).IsConnectedValue.SetValue(false);

        Assert.That(tile.SpeedDisplay, Is.EqualTo("  - SS >"));
    }
}
