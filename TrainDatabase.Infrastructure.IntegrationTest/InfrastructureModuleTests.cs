using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.IntegrationTest;

[TestFixture]
public class InfrastructureModuleTests
{
    [Test]
    public void Resolve_ClientAdapter_WiresZ21CommandStation()
    {
        ContainerBuilder builder = new();
        builder.RegisterGeneric(typeof(NullLogger<>)).As(typeof(ILogger<>)).SingleInstance();
        builder.RegisterModule(new InfrastructureModule());
        using IContainer container = builder.Build();

        IClientAdapter adapter = container.Resolve<IClientAdapter>();

        Assert.That(adapter, Is.Not.Null);
    }
}
