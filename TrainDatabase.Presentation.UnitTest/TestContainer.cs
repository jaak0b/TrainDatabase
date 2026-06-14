using Autofac;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Services;
using TrainDatabase.Presentation.Files;
using TrainDatabase.Presentation.UnitTest.Fakes;

namespace TrainDatabase.Presentation.UnitTest;

/// <summary>Builds an Autofac container over <see cref="PresentationModule"/> with fake Core ports.</summary>
internal sealed class TestContainer : IDisposable
{
    private readonly IContainer container;

    public FakeVehicleRepository Repository { get; } = new();

    public TestContainer(params Vehicle[] seed)
    {
        Repository.Seed(seed);

        ContainerBuilder builder = new();
        builder.RegisterModule<PresentationModule>();
        builder.RegisterInstance(Repository).As<IVehicleRepository>();
        builder.Register<VehiclePresenterFactory>(_ => id => new FakeVehiclePresenter(Repository.GetVehicleByIdRequired(id))).SingleInstance();
        builder.RegisterInstance(new FakeVehicleControlService()).As<IVehicleControlService>();
        builder.RegisterInstance(new FakeClientPresenter()).As<IClientPresenter>();
        builder.RegisterInstance(new FakeSettingsStore()).As<ISettingsStore>();
        builder.RegisterInstance(new FakeFilePicker(null)).As<IFilePicker>();
        builder.RegisterInstance(new FakeDatabaseImporter()).As<IDatabaseImporter>();
        builder.RegisterInstance(new FakeVehicleImageStore()).As<IVehicleImageStore>();

        container = builder.Build();
    }

    public T Resolve<T>() where T : notnull => container.Resolve<T>();

    public void Dispose() => container.Dispose();
}
