using Autofac;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Services;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class PresentationModuleTests
{
    private static IContainer BuildContainer()
    {
        ContainerBuilder builder = new();
        builder.RegisterModule<PresentationModule>();

        // Stub the Core ports the per-vehicle view models depend on.
        builder.Register<VehiclePresenterFactory>(_ => _ => new FakeVehiclePresenter(new Vehicle { Address = 3 })).SingleInstance();
        builder.RegisterInstance(new FakeVehicleControlService()).As<IVehicleControlService>();
        builder.RegisterInstance(new FakeClientPresenter()).As<IClientPresenter>();
        builder.RegisterInstance(new FakeVehicleRepository()).As<IVehicleRepository>();
        builder.RegisterInstance(new FakeSettingsStore()).As<ISettingsStore>();
        builder.RegisterInstance(new FakeFilePicker(null)).As<TrainDatabase.Presentation.Files.IFilePicker>();
        builder.RegisterInstance(new FakeDatabaseImporter()).As<IDatabaseImporter>();
        builder.RegisterInstance(new FakeVehicleImageStore()).As<IVehicleImageStore>();

        return builder.Build();
    }

    [Test]
    public void Factory_ReturnsCachedInstance_PerVehicleId()
    {
        using IContainer container = BuildContainer();
        VehicleManualControlViewModelFactory factory = container.Resolve<VehicleManualControlViewModelFactory>();

        VehicleManualControlViewModel first = factory(7);
        VehicleManualControlViewModel firstAgain = factory(7);
        VehicleManualControlViewModel second = factory(8);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.SameAs(firstAgain));
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(second.VehicleId, Is.EqualTo(8));
        });
    }

    [Test]
    public void Shell_ResolvesWithDefaultRoute()
    {
        using IContainer container = BuildContainer();
        ShellViewModel shell = container.Resolve<ShellViewModel>();

        Assert.That(shell.Current, Is.InstanceOf<VehicleTilePanelViewModel>());
    }

    [Test]
    public void Workspace_ResolvesAsSingleton()
    {
        using IContainer container = BuildContainer();

        VehicleWorkspaceViewModel first = container.Resolve<VehicleWorkspaceViewModel>();
        VehicleWorkspaceViewModel second = container.Resolve<VehicleWorkspaceViewModel>();

        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public void EditFactory_ReturnsCachedInstance_PerVehicleId()
    {
        using IContainer container = BuildContainer();
        VehicleEditViewModelFactory factory = container.Resolve<VehicleEditViewModelFactory>();

        VehicleEditViewModel first = factory(7);
        VehicleEditViewModel firstAgain = factory(7);
        Assert.That(first, Is.SameAs(firstAgain));
    }

    [Test]
    public void Shell_OpenWorkspace_SetsCurrentToWorkspace()
    {
        using IContainer container = BuildContainer();
        ShellViewModel shell = container.Resolve<ShellViewModel>();

        shell.OpenWorkspaceCommand.Execute(null);

        Assert.That(shell.Current, Is.InstanceOf<VehicleWorkspaceViewModel>());
    }
}
