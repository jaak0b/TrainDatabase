using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrainDatabase.Core;
using TrainDatabase.Core.Ports;
using TrainDatabase.Infrastructure;

namespace TrainDatabase.Composition;

/// <summary>
/// Builds the application's Autofac container from the Core and Infrastructure modules and
/// applies database migrations. Each platform head calls this with its own UI module(s).
/// Replaces the old single composition root + <c>ServiceLocator</c> usage.
/// </summary>
public static class Bootstrapper
{
    public static async Task<IServiceProvider> InitializeAsync(
        ILoggerFactory? loggerFactory = null,
        params Module[] extraModules)
    {
        ServiceCollection services = new();
        if (loggerFactory is not null)
        {
            services.AddSingleton(loggerFactory);
        }

        services.AddLogging();

        ContainerBuilder builder = new();
        builder.Populate(services);
        builder.RegisterModule(new InfrastructureModule());
        builder.RegisterModule(new CoreModule());
        foreach (Module module in extraModules)
        {
            builder.RegisterModule(module);
        }

        IContainer container = builder.Build();
        AutofacServiceProvider provider = new(container);

        await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();

        return provider;
    }
}
