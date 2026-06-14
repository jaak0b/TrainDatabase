using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TrainDatabase.Infrastructure.Entities;
using TrainDatabase.Infrastructure.Extensions;

namespace TrainDatabase.Infrastructure.Database;

/// <summary>
/// The EF Core database context. Options-only: the connection (SQLite file path) is
/// always supplied by the composing head via <see cref="DbContextOptions{Database}"/>
/// sourced from <c>IAppStorage</c>. There is intentionally no <c>OnConfiguring</c>
/// fallback, so this context never resolves a path itself and tests can inject a
/// temp-folder or in-memory database.
/// </summary>
public class TrainDbContext : DbContext
{
    public TrainDbContext(DbContextOptions<TrainDbContext> options) : base(options)
    {
    }

    public event EventHandler? CollectionChanged;

    public virtual DbSet<VehicleFunctionEntity> Functions => Set<VehicleFunctionEntity>();

    public virtual DbSet<VehicleEntity> Vehicles => Set<VehicleEntity>();

    public virtual DbSet<VehicleCalibrationDataEntity> VehicleCalibrationData => Set<VehicleCalibrationDataEntity>();

    public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity obj) where TEntity : class
    {
        EntityEntry<TEntity> result = await Set<TEntity>().AddAsync(obj);
        await SaveChangesAsync();
        CollectionChanged?.Invoke(this, EventArgs.Empty);
        return result;
    }

    public void InvokeCollectionChanged() => CollectionChanged?.Invoke(this, EventArgs.Empty);

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity obj)
    {
        EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
        SaveChanges();
        CollectionChanged?.Invoke(this, EventArgs.Empty);
        return value;
    }

    public async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>(TEntity obj) where TEntity : class
    {
        EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
        await SaveChangesAsync();
        CollectionChanged?.Invoke(this, EventArgs.Empty);
        return value;
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity obj)
    {
        EntityEntry<TEntity> result = Set<TEntity>().Update(obj);
        SaveChanges();
        CollectionChanged?.Invoke(this, EventArgs.Empty);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ValueConverter<decimal?[], string> decimArrayToStringConverter = new(
            v => string.Join(";", v),
            v => v.Split(";", StringSplitOptions.None).Select(val => val.IsDecimal() ? decimal.Parse(val) : (decimal?)null).ToArray());
        ValueComparer<decimal?[]> decimalArrayValueComparer = new(
            (a, b) => (a ?? Array.Empty<decimal?>()).SequenceEqual(b ?? Array.Empty<decimal?>()),
            v => v.Aggregate(0, (a, i) => HashCode.Combine(a, i.GetHashCode())),
            v => v.ToArray());

#pragma warning disable CS0612 // obsolete traction arrays retained for legacy data
        modelBuilder.Entity<VehicleEntity>().Property(e => e.TractionForward).HasConversion(decimArrayToStringConverter).Metadata.SetValueComparer(decimalArrayValueComparer);
        modelBuilder.Entity<VehicleEntity>().Property(e => e.TractionBackward).HasConversion(decimArrayToStringConverter).Metadata.SetValueComparer(decimalArrayValueComparer);
#pragma warning restore CS0612

        ValueConverter<List<int>, string> intListConverter = new(
            v => string.Join(";", v.Distinct()),
            v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(val => val.IsInt() ? int.Parse(val) : int.MinValue).Distinct().ToList());
        ValueComparer<List<int>> intListValueComparer = new(
            (a, b) => (a ?? new List<int>()).SequenceEqual(b ?? new List<int>()),
            v => v.Aggregate(0, (a, i) => HashCode.Combine(a, i.GetHashCode())),
            v => v.ToList());
        modelBuilder.Entity<VehicleEntity>().Property(e => e.TractionVehicleIds).HasConversion(intListConverter).Metadata.SetValueComparer(intListValueComparer);

        modelBuilder.Entity<VehicleCalibrationDataEntity>()
            .HasIndex(entity => new { entity.VehicleId, entity.Direction, entity.SpeedStep })
            .IsUnique();
    }

    /// <summary>Deletes all data from the tracked sets.</summary>
    public void DeleteAll()
    {
        DetachAllEntities();
        Functions.RemoveAll();
        Vehicles.RemoveAll();
        SaveChanges();
        InvokeCollectionChanged();
    }

    /// <summary>Detaches all tracked entities.</summary>
    public void DetachAllEntities()
    {
        List<EntityEntry> changedEntriesCopy = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        foreach (EntityEntry entry in changedEntriesCopy)
        {
            entry.State = EntityState.Detached;
        }
    }
}
