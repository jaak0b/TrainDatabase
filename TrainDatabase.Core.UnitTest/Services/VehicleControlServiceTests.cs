using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Services;
using TrainDatabase.Core.UnitTest.Fakes;

namespace TrainDatabase.Core.UnitTest.Services;

[TestFixture]
public class VehicleControlServiceTests
{
    [Test]
    public async Task SetVehicleSpeedAsync_NoConsist_SendsSingleCommand()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        VehicleControlService service = new(client, repository);
        Vehicle vehicle = new() { Address = 55 };

        await service.SetVehicleSpeedAsync(vehicle, speed: 30, direction: true);

        Assert.That(client.DriveCommands, Has.Count.EqualTo(1));
        LocoSetDriveData command = client.DriveCommands[0];
        Assert.Multiple(() =>
        {
            Assert.That(command.VehicleAddress, Is.EqualTo(55));
            Assert.That(command.Speed, Is.EqualTo(30));
            Assert.That(command.Direction, Is.True);
        });
    }

    [Test]
    public async Task SetVehicleSpeedAsync_WithConsist_DrivesLeadAndMembersAtSameSpeed()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        Vehicle member1 = new() { Id = 2, Address = 11 };
        Vehicle member2 = new() { Id = 3, Address = 12 };
        repository.Seed(member1, member2);
        Vehicle lead = new() { Id = 1, Address = 10, TractionVehicleIds = { 2, 3 } };
        VehicleControlService service = new(client, repository);

        await service.SetVehicleSpeedAsync(lead, speed: 40, direction: true);

        Assert.That(client.DriveCommands, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(client.DriveCommands.Select(c => (int)c.VehicleAddress), Is.EquivalentTo(new[] { 10, 11, 12 }));
            Assert.That(client.DriveCommands.Select(c => (int)c.Speed), Is.All.EqualTo(40));
            Assert.That(client.DriveCommands.Select(c => c.Direction), Is.All.True);
        });
    }

    [Test]
    public async Task SetVehicleSpeedAsync_NoConsist_CarriesVehicleRegulationStep()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        VehicleControlService service = new(client, repository);
        Vehicle vehicle = new() { Address = 55, RegulationStep = RegulationStep.Step28 };

        await service.SetVehicleSpeedAsync(vehicle, speed: 30, direction: true);

        Assert.That(client.DriveCommands[0].SpeedStep, Is.EqualTo(RegulationStep.Step28));
    }

    [Test]
    public async Task SetVehicleSpeedAsync_WithConsist_EachCommandCarriesItsOwnRegulationStep()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        Vehicle member = new() { Id = 2, Address = 11, RegulationStep = RegulationStep.Step14 };
        repository.Seed(member);
        Vehicle lead = new() { Id = 1, Address = 10, RegulationStep = RegulationStep.Step128, TractionVehicleIds = { 2 } };
        VehicleControlService service = new(client, repository);

        await service.SetVehicleSpeedAsync(lead, speed: 40, direction: true);

        LocoSetDriveData leadCommand = client.DriveCommands.Single(c => c.VehicleAddress == 10);
        LocoSetDriveData memberCommand = client.DriveCommands.Single(c => c.VehicleAddress == 11);
        Assert.Multiple(() =>
        {
            Assert.That(leadCommand.SpeedStep, Is.EqualTo(RegulationStep.Step128));
            Assert.That(memberCommand.SpeedStep, Is.EqualTo(RegulationStep.Step14));
        });
    }

    [Test]
    public async Task SetVehicleSpeedAsync_MemberWithInvertTraction_FlipsItsDirection()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        Vehicle member = new() { Id = 2, Address = 11, InvertTraction = true };
        repository.Seed(member);
        Vehicle lead = new() { Id = 1, Address = 10, TractionVehicleIds = { 2 } };
        VehicleControlService service = new(client, repository);

        await service.SetVehicleSpeedAsync(lead, speed: 40, direction: true);

        LocoSetDriveData leadCommand = client.DriveCommands.Single(c => c.VehicleAddress == 10);
        LocoSetDriveData memberCommand = client.DriveCommands.Single(c => c.VehicleAddress == 11);
        Assert.Multiple(() =>
        {
            Assert.That(leadCommand.Direction, Is.True);
            Assert.That(memberCommand.Direction, Is.False);
        });
    }

    [Test]
    public async Task SetVehicleSpeedAsync_MissingMember_IsSkipped()
    {
        FakeClientAdapter client = new();
        FakeVehicleRepository repository = new();
        Vehicle lead = new() { Id = 1, Address = 10, TractionVehicleIds = { 99 } };
        VehicleControlService service = new(client, repository);

        await service.SetVehicleSpeedAsync(lead, speed: 40, direction: true);

        Assert.That(client.DriveCommands, Has.Count.EqualTo(1));
        Assert.That(client.DriveCommands[0].VehicleAddress, Is.EqualTo(10));
    }
}
