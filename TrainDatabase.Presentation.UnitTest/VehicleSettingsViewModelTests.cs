using TrainDatabase.Core.Domain;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class VehicleSettingsViewModelTests
{
    private static TestContainer SeededConsist() => new(
        new Vehicle { Id = 1, Name = "Lead", TractionVehicleIds = { 2 } },
        new Vehicle { Id = 2, Name = "Member two" },
        new Vehicle { Id = 3, Name = "Member three" });

    [Test]
    public void Members_ExcludeSelf_AndSeedSelectionFromConsist()
    {
        using TestContainer test = SeededConsist();
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Members.Select(m => m.VehicleId), Is.EquivalentTo(new[] { 2, 3 }));
            Assert.That(settings.Members.Single(m => m.VehicleId == 2).IsSelected, Is.True);
            Assert.That(settings.Members.Single(m => m.VehicleId == 3).IsSelected, Is.False);
        });
    }

    [Test]
    public async Task Save_WritesCheckedMembersToTractionVehicleIds()
    {
        using TestContainer test = SeededConsist();
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        settings.Members.Single(m => m.VehicleId == 3).IsSelected = true;
        await settings.SaveCommand.ExecuteAsync(null);

        Assert.That(test.Repository.GetVehicleByIdRequired(1).TractionVehicleIds, Is.EquivalentTo(new[] { 2, 3 }));
    }

    [Test]
    public void Revert_RestoresSeededSelection()
    {
        using TestContainer test = SeededConsist();
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        settings.Members.Single(m => m.VehicleId == 3).IsSelected = true;
        settings.RevertCommand.Execute(null);

        Assert.That(settings.Members.Single(m => m.VehicleId == 3).IsSelected, Is.False);
    }

    [Test]
    public void Load_SeedsGeneralFieldsFromVehicle()
    {
        using TestContainer test = new(new Vehicle
        {
            Id = 1,
            Name = "Lead",
            Type = VehicleType.Wagen,
            RegulationStep = RegulationStep.Step28,
            InvertTraction = true,
            Description = "branch line",
        });
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Type, Is.EqualTo(VehicleType.Wagen));
            Assert.That(settings.RegulationStep, Is.EqualTo(RegulationStep.Step28));
            Assert.That(settings.InvertTraction, Is.True);
            Assert.That(settings.Description, Is.EqualTo("branch line"));
        });
    }

    [Test]
    public async Task Save_PersistsGeneralFields()
    {
        using TestContainer test = new(new Vehicle { Id = 1, Name = "Lead" });
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        settings.Type = VehicleType.Steuerwagen;
        settings.RegulationStep = RegulationStep.Step14;
        settings.InvertTraction = true;
        settings.Description = "in service";
        await settings.SaveCommand.ExecuteAsync(null);

        Vehicle saved = test.Repository.GetVehicleByIdRequired(1);
        Assert.Multiple(() =>
        {
            Assert.That(saved.Type, Is.EqualTo(VehicleType.Steuerwagen));
            Assert.That(saved.RegulationStep, Is.EqualTo(RegulationStep.Step14));
            Assert.That(saved.InvertTraction, Is.True);
            Assert.That(saved.Description, Is.EqualTo("in service"));
        });
    }

    [Test]
    public async Task Save_PersistsFunctionEdits_InTheSameCommand()
    {
        using TestContainer test = new(new Vehicle
        {
            Id = 1,
            Name = "Lead",
            Functions = { new VehicleFunction { Id = 7, Name = "Light", Address = 0 } },
        });
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        settings.Functions.Single().Name = "Cab light";
        await settings.SaveCommand.ExecuteAsync(null);

        Assert.That(test.Repository.FunctionUpdates.Any(update => update.VehicleId == 1), Is.True);
    }

    [Test]
    public void Revert_RestoresGeneralFields()
    {
        using TestContainer test = new(new Vehicle
        {
            Id = 1,
            Name = "Lead",
            Type = VehicleType.Lokomotive,
            Description = "original",
        });
        VehicleSettingsViewModel settings = test.Resolve<VehicleSettingsViewModelFactory>()(1);

        settings.Type = VehicleType.Wagen;
        settings.Description = "edited";
        settings.RevertCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Type, Is.EqualTo(VehicleType.Lokomotive));
            Assert.That(settings.Description, Is.EqualTo("original"));
        });
    }

    [Test]
    public void OptionLists_ExposeEveryEnumValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(VehicleSettingsViewModel.VehicleTypes, Is.EquivalentTo(Enum.GetValues<VehicleType>()));
            Assert.That(VehicleSettingsViewModel.RegulationSteps, Is.EquivalentTo(Enum.GetValues<RegulationStep>()));
        });
    }
}
