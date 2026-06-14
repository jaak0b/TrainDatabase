using TrainDatabase.Core.Domain;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Infrastructure.Repositories;

namespace TrainDatabase.Infrastructure.IntegrationTest;

[TestFixture]
public class VehicleRepositoryTests
{
    private TempDatabase db = null!;
    private VehicleRepository repository = null!;

    [SetUp]
    public void SetUp()
    {
        db = new TempDatabase();
        repository = new VehicleRepository(db.Context, db.Mapper);
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public void GetVehicleById_ReturnsMappedDomainVehicle()
    {
        VehicleEntity entity = new()
        {
            Name = "BR 101",
            Address = 42,
            Type = VehicleType.Lokomotive,
            RegulationStep = RegulationStep.Step128,
            Functions = { new VehicleFunctionEntity { Name = "Light", Address = 0, EnumType = FunctionType.Light1 } },
        };
        db.Context.Vehicles.Add(entity);
        db.Context.SaveChanges();

        Vehicle? result = repository.GetVehicleById(entity.Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Name, Is.EqualTo("BR 101"));
            Assert.That(result.Address, Is.EqualTo(42));
            Assert.That(result.Functions, Has.Count.EqualTo(1));
            Assert.That(result.Functions[0].EnumType, Is.EqualTo(FunctionType.Light1));
        });
    }

    [Test]
    public async Task UpdateVehicleFunctionsAsync_PersistsScalarChanges()
    {
        VehicleEntity entity = new()
        {
            Name = "Loco",
            Address = 3,
            Functions = { new VehicleFunctionEntity { Name = "old", Address = 1, ButtonType = ButtonType.Switch } },
        };
        db.Context.Vehicles.Add(entity);
        db.Context.SaveChanges();
        int functionId = entity.Functions[0].Id;

        await repository.UpdateVehicleFunctionsAsync(entity.Id, new[]
        {
            new VehicleFunction { Id = functionId, Name = "Cab light", Address = 5, ButtonType = ButtonType.PushButton, IsActive = true },
        });

        db.Context.ChangeTracker.Clear();
        VehicleFunctionEntity reloaded = db.Context.Functions.Single(f => f.Id == functionId);
        Assert.Multiple(() =>
        {
            Assert.That(reloaded.Name, Is.EqualTo("Cab light"));
            Assert.That(reloaded.Address, Is.EqualTo(5));
            Assert.That(reloaded.ButtonType, Is.EqualTo(ButtonType.PushButton));
        });
    }

    [Test]
    public async Task UpdateVehicleAsync_PersistsTractionVehicleIds()
    {
        VehicleEntity lead = new() { Name = "Lead", Address = 10 };
        VehicleEntity firstMember = new() { Name = "Member 1", Address = 11 };
        VehicleEntity secondMember = new() { Name = "Member 2", Address = 12 };
        db.Context.Vehicles.AddRange(lead, firstMember, secondMember);
        db.Context.SaveChanges();

        Vehicle leadDomain = repository.GetVehicleByIdRequired(lead.Id);
        leadDomain.TractionVehicleIds = new List<int> { firstMember.Id, secondMember.Id };
        await repository.UpdateVehicleAsync(leadDomain);

        db.Context.ChangeTracker.Clear();
        Vehicle reloaded = repository.GetVehicleByIdRequired(lead.Id);
        Assert.That(reloaded.TractionVehicleIds, Is.EquivalentTo(new[] { firstMember.Id, secondMember.Id }));
    }

    [Test]
    public async Task AddVehicleAsync_PersistsTractionVehicleIds()
    {
        VehicleEntity member = new() { Name = "Member", Address = 11 };
        db.Context.Vehicles.Add(member);
        db.Context.SaveChanges();

        int newId = await repository.AddVehicleAsync(new Vehicle { Name = "Lead", Address = 10, TractionVehicleIds = { member.Id } });

        db.Context.ChangeTracker.Clear();
        Assert.That(repository.GetVehicleByIdRequired(newId).TractionVehicleIds, Is.EquivalentTo(new[] { member.Id }));
    }

    [Test]
    public async Task UpdateVehicleAsync_RemovesDeselectedMembers()
    {
        VehicleEntity lead = new() { Name = "Lead", Address = 10 };
        VehicleEntity first = new() { Name = "M1", Address = 11 };
        VehicleEntity second = new() { Name = "M2", Address = 12 };
        db.Context.Vehicles.AddRange(lead, first, second);
        db.Context.SaveChanges();

        Vehicle leadDomain = repository.GetVehicleByIdRequired(lead.Id);
        leadDomain.TractionVehicleIds = new List<int> { first.Id, second.Id };
        await repository.UpdateVehicleAsync(leadDomain);

        leadDomain = repository.GetVehicleByIdRequired(lead.Id);
        leadDomain.TractionVehicleIds = new List<int> { first.Id };
        await repository.UpdateVehicleAsync(leadDomain);

        db.Context.ChangeTracker.Clear();
        Assert.That(repository.GetVehicleByIdRequired(lead.Id).TractionVehicleIds, Is.EquivalentTo(new[] { first.Id }));
    }

    [Test]
    public async Task DeletingMemberVehicle_RemovesItFromLeadConsist()
    {
        VehicleEntity lead = new() { Name = "Lead", Address = 10 };
        VehicleEntity member = new() { Name = "Member", Address = 11 };
        db.Context.Vehicles.AddRange(lead, member);
        db.Context.SaveChanges();

        Vehicle leadDomain = repository.GetVehicleByIdRequired(lead.Id);
        leadDomain.TractionVehicleIds = new List<int> { member.Id };
        await repository.UpdateVehicleAsync(leadDomain);

        await repository.DeleteVehicleAsync(member.Id);

        db.Context.ChangeTracker.Clear();
        Assert.That(repository.GetVehicleByIdRequired(lead.Id).TractionVehicleIds, Does.Not.Contain(member.Id));
    }

    [Test]
    public void GetVehicleById_ReturnsNull_WhenMissing()
    {
        Assert.That(repository.GetVehicleById(999), Is.Null);
    }

    [Test]
    public void GetVehicleByIdRequired_Throws_WhenMissing()
    {
        Assert.Throws<IdNotFoundException>(() => repository.GetVehicleByIdRequired(999));
    }

    [Test]
    public async Task UpdateVehicleAsync_PersistsChanges_AndPublishesToStream()
    {
        VehicleEntity entity = new() { Name = "Old", Address = 7 };
        db.Context.Vehicles.Add(entity);
        db.Context.SaveChanges();

        Vehicle changed = repository.GetVehicleByIdRequired(entity.Id);
        changed.Name = "New";

        Vehicle? streamed = null;
        using (repository.VehicleChangedStream.Subscribe(v => streamed = v))
        {
            await repository.UpdateVehicleAsync(changed);
        }

        Assert.Multiple(() =>
        {
            Assert.That(repository.GetVehicleByIdRequired(entity.Id).Name, Is.EqualTo("New"));
            Assert.That(streamed, Is.Not.Null);
            Assert.That(streamed!.Name, Is.EqualTo("New"));
        });
    }
}
