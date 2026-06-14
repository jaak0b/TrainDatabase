using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleManagementViewModelTests
{
    private static VehicleManagementViewModel Build(IVehicleRepository repository, FakeDialogService dialogs, NavigationService nav)
    {
        VehicleDetailViewModelFactory detailFactory = id =>
            throw new InvalidOperationException($"detail factory not expected (id {id})");
        return new VehicleManagementViewModel(repository, dialogs, nav, detailFactory);
    }

    [Test]
    public async Task Delete_WhenConfirmed_RemovesVehicleAndRefreshes()
    {
        FakeVehicleRepository repository = new();
        await repository.AddVehicleAsync(new Vehicle { Name = "Keep", Address = 1 });
        int removeId = await repository.AddVehicleAsync(new Vehicle { Name = "Remove", Address = 2 });

        VehicleManagementViewModel vm = Build(repository, new FakeDialogService { ConfirmResult = true }, new NavigationService());
        Assert.That(vm.Vehicles, Has.Count.EqualTo(2));

        VehicleListItem toRemove = vm.Vehicles.First(v => v.Id == removeId);
        await vm.DeleteCommand.ExecuteAsync(toRemove);

        Assert.That(vm.Vehicles.Select(v => v.Id), Does.Not.Contain(removeId));
        Assert.That(vm.Vehicles, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Delete_WhenNotConfirmed_KeepsVehicle()
    {
        FakeVehicleRepository repository = new();
        int id = await repository.AddVehicleAsync(new Vehicle { Name = "Stay", Address = 1 });

        VehicleManagementViewModel vm = Build(repository, new FakeDialogService { ConfirmResult = false }, new NavigationService());
        await vm.DeleteCommand.ExecuteAsync(vm.Vehicles.Single());

        Assert.That(vm.Vehicles.Select(v => v.Id), Does.Contain(id));
    }
}
