namespace TrainDatabase.Core.Ports;

/// <summary>
/// Platform-specific application storage locations. Replaces the Windows-bound
/// <c>Helper.Configuration.ApplicationData</c> paths. Implemented per head
/// (Desktop: %APPDATA%; Android: app-private files; Browser: virtual/IndexedDB).
/// Paths are plain strings to stay portable across platforms.
/// </summary>
public interface IAppStorage
{
    /// <summary>Full path to the SQLite database file.</summary>
    string DatabaseFilePath { get; }

    /// <summary>Directory where vehicle images are stored.</summary>
    string VehicleImageDirectory { get; }

    /// <summary>Directory where log files are written.</summary>
    string LogDirectory { get; }

    /// <summary>Creates and returns a fresh, unique temporary directory.</summary>
    string CreateTempDirectory();
}
