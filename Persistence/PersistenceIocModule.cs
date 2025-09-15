using System.IO;
using Autofac;
using Helper;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
  public class PersistenceIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.Register(c =>
                       {
                         string dbPath = Configuration.ApplicationData.DatabaseFile.FullName;
                         Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                         DbContextOptionsBuilder<Database.Database> optionsBuilder = new();
                         if (!optionsBuilder.IsConfigured)
                         {
                           optionsBuilder.UseSqlite($"Data Source={dbPath}");
                         }

                         return new Database.Database(optionsBuilder.Options);
                       })
             .AsSelf()
             .InstancePerLifetimeScope();
    }
  }
}