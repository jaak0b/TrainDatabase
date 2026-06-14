using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleDetailViewModelTests
{
    [Test]
    public void Title_ReflectsVehicleName()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        VehicleDetailViewModel pane = test.Resolve<VehicleDetailViewModelFactory>()(7);

        Assert.That(pane.Title, Is.EqualTo("BR 218"));
    }

    [Test]
    public void TractionCount_ReflectsConsistSize()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "Lead", TractionVehicleIds = { 2, 3 } });
        VehicleDetailViewModel pane = test.Resolve<VehicleDetailViewModelFactory>()(7);

        Assert.That(pane.TractionCount, Is.EqualTo(2));
    }

    [Test]
    public void Edit_NavigatesToEditRouteForSameVehicle()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        NavigationService navigation = test.Resolve<NavigationService>();
        VehicleDetailViewModel pane = test.Resolve<VehicleDetailViewModelFactory>()(7);

        pane.EditCommand.Execute(null);

        Assert.That(navigation.Current, Is.InstanceOf<VehicleEditViewModel>());
        Assert.That(((VehicleEditViewModel)navigation.Current!).VehicleId, Is.EqualTo(7));
    }

    [Test]
    public void Close_RemovesSelfFromWorkspace()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        workspace.OpenVehicle(7);
        VehicleDetailViewModel pane = workspace.Panes[0];

        pane.CloseCommand.Execute(null);

        Assert.That(workspace.Panes, Is.Empty);
    }
}
