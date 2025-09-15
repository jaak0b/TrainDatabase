using System.IO;
using Autofac;
using AutoMapper;
using Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Mapping;

namespace Persistence
{
  public class PersistenceIocModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.Register(context =>
                       {
                         ILoggerFactory loggerFactory = context.Resolve<ILoggerFactory>();
                         MapperConfiguration config = new(mapperConfigurationExpression => { mapperConfigurationExpression.AddProfile<VehicleProfile>(); }, loggerFactory);
                         config.AssertConfigurationIsValid();
                         return config;
                       })
             .SingleInstance();

      builder.Register(context =>
                       {
                         MapperConfiguration config = context.Resolve<MapperConfiguration>();
                         return config.CreateMapper(context.Resolve);
                       })
             .As<IMapper>()
             .InstancePerLifetimeScope();

      builder.Register(context =>
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