using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Database;

/// <summary>
/// Applies EF Core migrations to bring the database schema up to date. Replaces the
/// single <c>db.Database.Migrate()</c> call that used to live in the composition
/// bootstrapper; each platform head calls this once during startup.
///
/// Handles the rewrite cut-over: a database created before the rewrite already has the
/// app schema but uses the old migration history. Rather than fail trying to recreate
/// existing tables, the baseline (<c>InitialCreate</c>) migration is stamped as applied
/// when the schema is already present, after which normal migration continues.
/// </summary>
public sealed class DatabaseInitializer(TrainDbContext context) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IHistoryRepository history = context.GetService<IHistoryRepository>();
        IMigrationsAssembly migrationsAssembly = context.GetService<IMigrationsAssembly>();

        string? baselineId = migrationsAssembly.Migrations.Keys.FirstOrDefault();
        if (baselineId is not null && await SchemaAlreadyPresentAsync(cancellationToken))
        {
            IReadOnlyList<HistoryRow> applied = history.Exists()
                ? history.GetAppliedMigrations()
                : Array.Empty<HistoryRow>();

            if (applied.All(row => row.MigrationId != baselineId))
            {
                if (!history.Exists())
                {
                    await context.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript(), cancellationToken);
                }

                string version = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "9.0.0";
                await context.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(baselineId, version)), cancellationToken);
            }
        }

        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// True when the core schema already exists (a pre-rewrite or already-migrated database).
    /// </summary>
    private async Task<bool> SchemaAlreadyPresentAsync(CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using System.Data.Common.DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Vehicles';";
            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result) > 0;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
