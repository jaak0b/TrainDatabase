using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.UnitTest.Reactive;

[TestFixture]
public class ObservableValueTests
{
    [Test]
    public void Value_ReturnsInitialValue()
    {
        using ObservableValue<int> value = new(7);
        Assert.That(value.Value, Is.EqualTo(7));
    }

    [Test]
    public void Subscribe_ReceivesCurrentValueImmediately()
    {
        using ObservableValue<string> value = new("a");

        string? received = null;
        using (value.Subscribe(v => received = v))
        {
            Assert.That(received, Is.EqualTo("a"));
        }
    }

    [Test]
    public void SetValue_UpdatesValueAndNotifiesSubscribers()
    {
        using ObservableValue<int> value = new(0);
        List<int> received = new();
        using (value.Subscribe(received.Add))
        {
            value.SetValue(1);
            value.SetValue(2);
        }

        Assert.Multiple(() =>
        {
            Assert.That(value.Value, Is.EqualTo(2));
            Assert.That(received, Is.EqualTo(new[] { 0, 1, 2 }));
        });
    }

    [Test]
    public void SetValue_AfterDispose_DoesNotThrowToCaller()
    {
        ObservableValue<int> value = new(0);
        value.Dispose();
        Assert.DoesNotThrow(() => value.SetValue(1));
    }
}
