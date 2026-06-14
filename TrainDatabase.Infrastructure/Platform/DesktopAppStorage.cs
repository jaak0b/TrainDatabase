using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Platform;

/// <summary>
/// Desktop implementation of <see cref="IAppStorage"/>. Paths live under the per-user
/// application-data folder (<c>%APPDATA%\TrainDatabase</c> by default), preserving the
/// pre-rewrite layout. The base directory is injectable so tests never touch real
/// application data.
/// </summary>
public sealed class DesktopAppStorage : IAppStorage
{
    private readonly string baseDirectory;

    public DesktopAppStorage(string? baseDirectory = null)
    {
        // Precedence: explicit arg > TRAINDATABASE_DATA_DIR env var (handy for throwaway/dev
        // runs) > the per-user application-data folder.
        this.baseDirectory = baseDirectory
            ?? Environment.GetEnvironmentVariable("TRAINDATABASE_DATA_DIR")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrainDatabase");
    }

    public string DatabaseFilePath => Path.Combine(baseDirectory, "Data", "Database.sqlite");

    public string VehicleImageDirectory => Path.Combine(baseDirectory, "Data", "VehicleImage");

    public string LogDirectory => Path.Combine(baseDirectory, "Log");

    public string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "TrainDatabase", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
