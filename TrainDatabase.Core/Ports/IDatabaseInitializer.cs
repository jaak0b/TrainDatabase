namespace TrainDatabase.Core.Ports;

/// <summary>
/// Prepares the database for use (applies EF Core migrations). Replaces the single
/// <c>db.Database.Migrate()</c> call in the former <c>Composition.Bootstrapper</c>;
/// each platform head invokes this once during composition, before the first route loads.
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
