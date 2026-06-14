namespace TrainDatabase.Core.Ports;

/// <summary>
/// Persistent key/value application settings. Replaces the Windows-bound
/// <c>ConfigurationManager</c>/app.config usage in <c>Helper.Configuration</c>.
/// Implemented per head (Desktop/Android: JSON file; Browser: localStorage).
/// Typed accessors are provided by <see cref="SettingsStoreExtensions"/>.
/// </summary>
public interface ISettingsStore
{
    /// <summary>Returns the raw string value for <paramref name="key"/>, or <c>null</c> if unset.</summary>
    string? Get(string key);

    /// <summary>Sets (or, when <paramref name="value"/> is <c>null</c>, clears) the value for <paramref name="key"/>.</summary>
    void Set(string key, string? value);
}
