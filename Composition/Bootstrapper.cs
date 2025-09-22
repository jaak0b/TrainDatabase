using Adapter;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Database;
using Serilog;

namespace Composition
{
  public static class Bootstrapper
  {
    public static IServiceProvider Initialize(IModule? module, ILogger? logger)
    {
      ServiceCollection serviceCollection = new();

      if (logger is not null)
        serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true));

      ContainerBuilder builder = new();
      builder.Populate(serviceCollection);

      builder.RegisterModule(new AdapterModule());
      builder.RegisterModule(new PersistenceIocModule());
      builder.RegisterModule(new CoreIocModule());
      if (module is not null)
        builder.RegisterModule(module);

      IContainer container = builder.Build();
      ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

      using ILifetimeScope scope = container.BeginLifetimeScope();
      Database db = scope.Resolve<Database>();
      db.Database.Migrate();

      return new AutofacServiceProvider(container);
    }
  }
}