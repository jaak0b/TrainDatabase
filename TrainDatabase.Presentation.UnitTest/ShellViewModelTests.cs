using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class ShellViewModelTests
{
    [Test]
    public void IsDisconnected_FlowsFromClientPresenter()
    {
        using TestContainer test = new();
        ShellViewModel shell = test.Resolve<ShellViewModel>();
        FakeClientPresenter client = (FakeClientPresenter)test.Resolve<IClientPresenter>();

        Assert.That(shell.IsDisconnected, Is.False);

        client.IsDisconnectedValue.SetValue(true);

        Assert.That(shell.IsDisconnected, Is.True);
    }
}
