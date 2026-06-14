using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrainDatabase.Infrastructure.Database;

/// <summary>
/// Lets the EF Core tooling (<c>dotnet ef</c>) construct the options-only
/// <see cref="TrainDbContext"/> at design time for generating migrations.
/// Not used at runtime — heads supply their own options.
/// </summary>
public sealed class DesignTimeTrainDbContextFactory : IDesignTimeDbContextFactory<TrainDbContext>
{
    public TrainDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<TrainDbContext> options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseSqlite("Data Source=designtime.sqlite")
            .Options;
        return new TrainDbContext(options);
    }
}
