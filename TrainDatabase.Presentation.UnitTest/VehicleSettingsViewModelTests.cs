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
}
