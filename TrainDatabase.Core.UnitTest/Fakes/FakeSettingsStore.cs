using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.UnitTest.Fakes;

public sealed class FakeSettingsStore : ISettingsStore
{
    private readonly Dictionary<string, string> values = new();

    public string? Get(string key) => values.TryGetValue(key, out string? value) ? value : null;

    public void Set(string key, string? value)
    {
        if (value is null)
        {
            values.Remove(key);
        }
        else
        {
            values[key] = value;
        }
    }
}
