using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleEditViewModelTests
{
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
