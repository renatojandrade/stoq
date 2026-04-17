using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Infrastructure.Database;
using Stoq.Catalog.Infrastructure.Persistence;
using Stoq.Catalog.Infrastructure.Persistence.Interceptors;
using Stoq.Catalog.Infrastructure.Persistence.Repositories;
using Stoq.Catalog.Infrastructure.Services;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Catalog.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IProductRepository, ProductRepositoryImpl>();

#pragma warning disable S1075 // Well-known public API base URLs — not configurable resource paths.
        services.AddHttpClient<IOpenFoodFactsService, OpenFoodFactsService>(client =>
        {
            client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
            client.DefaultRequestHeaders.Add("User-Agent", "Stoq/1.0 (contact@stoq.app)");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<ICosmosProductService, CosmosProductService>(client =>
        {
            client.BaseAddress = new Uri("https://api.cosmos.bluesoft.com.br/");
            client.Timeout = TimeSpan.FromSeconds(10);

            var apiKey = configuration["Catalog:CosmosApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("X-Cosmos-Token", apiKey);
            }
        });
#pragma warning restore S1075

        return services;
    }

    public static IServiceCollection AddCatalogDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        services.AddScoped<CatalogDataSeeder>();

        services.AddDbContext<CatalogModuleDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IModuleDatabaseMigrator, CatalogDatabaseMigrator>();

        return services;
    }
}
