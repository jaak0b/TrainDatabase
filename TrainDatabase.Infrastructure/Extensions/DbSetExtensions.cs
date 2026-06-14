using Microsoft.EntityFrameworkCore;

namespace TrainDatabase.Infrastructure.Extensions;

internal static class DbSetExtensions
{
    public static void RemoveAll<TEntity>(this DbSet<TEntity> set) where TEntity : class
    {
        foreach (TEntity item in set.ToList())
        {
            set.Remove(item);
        }
    }
}
