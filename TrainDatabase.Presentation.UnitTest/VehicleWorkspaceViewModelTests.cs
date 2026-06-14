using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleWorkspaceViewModelTests
{
    [Test]
    public void OpenVehicle_AddsPane_AndActivatesIt()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        workspace.OpenVehicle(7);

        Assert.Multiple(() =>
        {
            Assert.That(workspace.Panes, Has.Count.EqualTo(1));
            Assert.That(workspace.Panes[0].VehicleId, Is.EqualTo(7));
            Assert.That(workspace.ActivePane, Is.SameAs(workspace.Panes[0]));
        });
    }

    [Test]
    public void OpenVehicle_SameId_DoesNotDuplicate_ActivatesExisting()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        workspace.OpenVehicle(7);
        VehicleDetailViewModel first = workspace.Panes[0];
        workspace.OpenVehicle(7);

        Assert.Multiple(() =>
        {
            Assert.That(workspace.Panes, Has.Count.EqualTo(1));
            Assert.That(workspace.ActivePane, Is.SameAs(first));
        });
    }

    [Test]
    public void OpenVehicle_DistinctIds_AddsMultiplePanes()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        workspace.OpenVehicle(7);
        workspace.OpenVehicle(8);

        Assert.That(workspace.Panes.Select(p => p.VehicleId), Is.EquivalentTo(new[] { 7, 8 }));
    }

    [Test]
    public void ClosePane_RemovesFromCollection()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        workspace.OpenVehicle(7);

        workspace.ClosePane(workspace.Panes[0]);

        Assert.That(workspace.Panes, Is.Empty);
    }

    [Test]
    public void ClosePane_ActivePane_FallsBackToRemainingPane()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        workspace.OpenVehicle(7);
        workspace.OpenVehicle(8);

        workspace.ClosePane(workspace.Panes.Single(pane => pane.VehicleId == 8));

        Assert.That(workspace.ActivePane!.VehicleId, Is.EqualTo(7));
    }

    [Test]
    public void ClosePane_LastPane_ClearsActivePane()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        workspace.OpenVehicle(7);

        workspace.ClosePane(workspace.Panes[0]);

        Assert.That(workspace.ActivePane, Is.Null);
    }

    [Test]
    public void OpenVehicleByIdCommand_OpensPane()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        workspace.OpenVehicleByIdCommand.Execute(7);

        Assert.That(workspace.Panes.Select(pane => pane.VehicleId), Is.EqualTo(new[] { 7 }));
    }

    [Test]
    public void AvailableVehicles_ExcludesAlreadyOpenTrains()
    {
        using TestContainer test = new(
            new Vehicle { Id = 7, Name = "Open", Position = 1 },
            new Vehicle { Id = 8, Name = "Closed", Position = 2 });
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        workspace.OpenVehicle(7);

        Assert.That(workspace.AvailableVehicles.Select(vehicle => vehicle.Id), Is.EqualTo(new[] { 8 }));
    }

    [Test]
    public void AvailableVehicles_OrderedByPosition()
    {
        using TestContainer test = new(
            new Vehicle { Id = 1, Name = "Second", Position = 2 },
            new Vehicle { Id = 2, Name = "First", Position = 1 });
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();

        Assert.That(workspace.AvailableVehicles.Select(vehicle => vehicle.Id), Is.EqualTo(new[] { 2, 1 }));
    }

    [Test]
    public void ClosePane_ThenReopen_ReturnsSameCachedInstance()
    {
        using TestContainer test = new();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        workspace.OpenVehicle(7);
        VehicleDetailViewModel first = workspace.Panes[0];

        workspace.ClosePane(first);
        workspace.OpenVehicle(7);

        Assert.That(workspace.Panes[0], Is.SameAs(first));
    }
}
