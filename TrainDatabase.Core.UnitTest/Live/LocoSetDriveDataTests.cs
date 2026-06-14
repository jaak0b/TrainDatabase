using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;

namespace TrainDatabase.Core.UnitTest.Live;

[TestFixture]
public class LocoSetDriveDataTests
{
    [Test]
    public void SpeedStep_WhenNotSet_DefaultsToStep128()
    {
        LocoSetDriveData data = new();

        Assert.That(data.SpeedStep, Is.EqualTo(RegulationStep.Step128));
    }
}
