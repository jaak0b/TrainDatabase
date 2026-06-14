using Microsoft.EntityFrameworkCore;
using TrainDatabase.Infrastructure.Database;
using TrainDatabase.Infrastructure.Mapping;

namespace TrainDatabase.Infrastructure.IntegrationTest;

/// <summary>
/// Creates a throwaway SQLite database under a unique temp folder, satisfying the hard
/// rule that no test ever touches the developer's real database. The file and folder are
/// deleted on <see cref="Dispose"/>.
/// </summary>
public sealed class TempDatabase : IDisposable
{
    private readonly string directory;

    public TempDatabase(bool initialize = true)
    {
        directory = Path.Combine(Path.GetTempPath(), "TrainDatabase.Tests", Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);
        string dbPath = Path.Combine(directory, "test.sqlite");

        DbContextOptions<TrainDbContext> options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        Context = new TrainDbContext(options);

        // Exercise the real migrations (not EnsureCreated) so the re-homed migration
        // history is covered. Pass initialize: false to drive IDatabaseInitializer directly.
        if (initialize)
        {
            Context.Database.Migrate();
        }

        Mapper = new MapsterEntityMapper(MappingConfig.Create());
    }

    public TrainDbContext Context { get; }

    public IEntityMapper Mapper { get; }

    public void Dispose()
    {
        Context.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try
        {
            if (System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; a locked file should not fail the test run.
        }
    }
}
