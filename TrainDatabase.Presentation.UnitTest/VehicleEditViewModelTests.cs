using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleEditViewModelTests
{
    [Test]
    public async Task Delete_Confirmed_RemovesVehicle_ClosesOpenPane_AndNavigatesBack()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "Scrap", Address = 3 });
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        NavigationService navigation = test.Resolve<NavigationService>();
        workspace.OpenVehicle(7);
        navigation.NavigateTo(workspace);
        VehicleEditViewModel edit = test.Resolve<VehicleEditViewModelFactory>()(7);
        navigation.NavigateTo(edit);

        await edit.DeleteCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(test.Repository.FullTextSearchVehicles("").Select(v => v.Id), Does.Not.Contain(7));
            Assert.That(workspace.Panes, Is.Empty);
            Assert.That(navigation.Current, Is.SameAs(workspace));
        });
    }

    [Test]
    public async Task Delete_Cancelled_KeepsVehicle()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "Keep", Address = 3 });
        FakeDialogService dialogs = (FakeDialogService)test.Resolve<IDialogService>();
        dialogs.ConfirmResult = false;
        VehicleEditViewModel edit = test.Resolve<VehicleEditViewModelFactory>()(7);

        await edit.DeleteCommand.ExecuteAsync(null);

        Assert.That(test.Repository.FullTextSearchVehicles("").Select(v => v.Id), Does.Contain(7));
    }

    [Test]
    public void HostsSettingsForSameVehicle()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        VehicleEditViewModel edit = test.Resolve<VehicleEditViewModelFactory>()(7);

        Assert.That(edit.Settings.VehicleId, Is.EqualTo(7));
    }

    [Test]
    public void Title_ReflectsVehicleName()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        VehicleEditViewModel edit = test.Resolve<VehicleEditViewModelFactory>()(7);

        Assert.That(edit.Title, Is.EqualTo("BR 218"));
    }

    [Test]
    public void Back_ReturnsToPreviousRoute()
    {
        using TestContainer test = new(new Vehicle { Id = 7, Name = "BR 218" });
        NavigationService navigation = test.Resolve<NavigationService>();
        VehicleWorkspaceViewModel workspace = test.Resolve<VehicleWorkspaceViewModel>();
        navigation.NavigateTo(workspace);
        VehicleEditViewModel edit = test.Resolve<VehicleEditViewModelFactory>()(7);
        navigation.NavigateTo(edit);

        edit.BackCommand.Execute(null);

        Assert.That(navigation.Current, Is.SameAs(workspace));
    }
}
