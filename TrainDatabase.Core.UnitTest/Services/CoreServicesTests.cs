using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Services;
using TrainDatabase.Core.UnitTest.Fakes;

namespace TrainDatabase.Core.UnitTest.Services;

[TestFixture]
public class CoreServicesTests
{
    [Test]
    public async Task TrackService_ForwardsPowerCommand()
    {
        FakeClientAdapter client = new();
        TrackService service = new(client);

        await service.SetTrackPowerAsync(true);
        await service.SetTrackPowerAsync(false);

        Assert.That(client.TrackPowerCommands, Is.EqualTo(new[] { true, false }));
    }

    [Test]
    public void ClientPresenter_DerivesDisconnectedFromConnected()
    {
        FakeClientAdapter client = new();
        ClientPresenter presenter = new(client);

        Assert.Multiple(() =>
        {
            Assert.That(presenter.IsConnected.Value, Is.False);
            Assert.That(presenter.IsDisconnected.Value, Is.True);
        });

        client.IsConnectedValue.SetValue(true);

        Assert.Multiple(() =>
        {
            Assert.That(presenter.IsConnected.Value, Is.True);
            Assert.That(presenter.IsDisconnected.Value, Is.False);
        });
    }

    [Test]
    public void TrackPresenter_ExposesAdapterTrackPower()
    {
        FakeClientAdapter client = new();
        TrackPresenter presenter = new(client);
        client.TrackPowerValue.SetValue(TrackPower.On);

        Assert.That(presenter.TrackPower.Value, Is.EqualTo(TrackPower.On));
    }

    [TestCase(0.0, 0)]
    [TestCase(5.0, 127)]
    [TestCase(2.5, 63)]
    [TestCase(10.0, 127)]
    [TestCase(-1.0, 0)]
    public void TrainPhysics_ConvertVelocityToDccSpeed_ClampsToRange(double velocity, int expected)
    {
        Assert.That(TrainPhysicsService.ConvertVelocityToDccSpeed(velocity), Is.EqualTo(expected));
    }
}
