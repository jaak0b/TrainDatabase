using System.Net;
using Microsoft.Extensions.Logging;
using TrainDatabase.Core.Services;
using TrainDatabase.Core.UnitTest.Fakes;

namespace TrainDatabase.Core.UnitTest.Services;

[TestFixture]
public class ConnectionInitializerTests
{
    [Test]
    public async Task ConnectAsync_ValidIp_ConnectsOnDefaultZ21Port()
    {
        FakeClientAdapter client = new();
        FakeSettingsStore settings = new();
        settings.Set("ClientIP", "192.168.0.111");
        ConnectionInitializer initializer = new(client, settings, new FakeLogger<ConnectionInitializer>());

        await initializer.ConnectAsync();

        Assert.That(client.ConnectedEndPoint, Is.EqualTo(new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105)));
    }

    [Test]
    public async Task ConnectAsync_NoSetting_DoesNotConnect()
    {
        FakeClientAdapter client = new();
        ConnectionInitializer initializer = new(client, new FakeSettingsStore(), new FakeLogger<ConnectionInitializer>());

        await initializer.ConnectAsync();

        Assert.That(client.ConnectedEndPoint, Is.Null);
    }

    [Test]
    public async Task ConnectAsync_BlankSetting_DoesNotConnect()
    {
        FakeClientAdapter client = new();
        FakeSettingsStore settings = new();
        settings.Set("ClientIP", "   ");
        ConnectionInitializer initializer = new(client, settings, new FakeLogger<ConnectionInitializer>());

        await initializer.ConnectAsync();

        Assert.That(client.ConnectedEndPoint, Is.Null);
    }

    [Test]
    public async Task ConnectAsync_GarbageSetting_DoesNotConnect()
    {
        FakeClientAdapter client = new();
        FakeSettingsStore settings = new();
        settings.Set("ClientIP", "not-an-ip");
        ConnectionInitializer initializer = new(client, settings, new FakeLogger<ConnectionInitializer>());

        await initializer.ConnectAsync();

        Assert.That(client.ConnectedEndPoint, Is.Null);
    }

    [Test]
    public async Task ConnectAsync_GarbageSetting_LogsWarning()
    {
        FakeClientAdapter client = new();
        FakeSettingsStore settings = new();
        settings.Set("ClientIP", "not-an-ip");
        FakeLogger<ConnectionInitializer> logger = new();
        ConnectionInitializer initializer = new(client, settings, logger);

        await initializer.ConnectAsync();

        Assert.That(logger.Entries, Does.Contain(LogLevel.Warning));
    }

    [Test]
    public async Task ConnectAsync_NoSetting_LogsNothing()
    {
        FakeClientAdapter client = new();
        FakeLogger<ConnectionInitializer> logger = new();
        ConnectionInitializer initializer = new(client, new FakeSettingsStore(), logger);

        await initializer.ConnectAsync();

        Assert.That(logger.Entries, Is.Empty);
    }
}
