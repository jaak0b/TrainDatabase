using System.Text.Json;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Platform;

/// <summary>
/// File-backed <see cref="ISettingsStore"/> storing key/value pairs as JSON. Replaces the
/// Windows-only <c>ConfigurationManager</c>/app.config usage. Suitable for Desktop and
/// Android (the Browser head uses a localStorage-backed implementation).
/// </summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string filePath;
    private readonly Dictionary<string, string> values;

    public JsonSettingsStore(string filePath)
    {
        this.filePath = filePath;
        values = Load(filePath);
    }

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

        Save();
    }

    private static Dictionary<string, string> Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                   ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            // Corrupt settings file: start fresh rather than crash the app.
            return new Dictionary<string, string>();
        }
    }

    private void Save()
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, JsonSerializer.Serialize(values, SerializerOptions));
    }
}
