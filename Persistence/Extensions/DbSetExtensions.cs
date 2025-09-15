using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Extensions
{
  public static class DbSetExtensions
  {
    public static void RemoveAll<TEntity>(this DbSet<TEntity> e) where TEntity : class
    {
      foreach (TEntity? item in e.ToList())
      {
        e.Remove(item);
      }
    }
  }
}