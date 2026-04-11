using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Infrastructure.Persistence;
using Stoq.Catalog.Infrastructure.Persistence.Interceptors;
using Stoq.Catalog.Infrastructure.Persistence.Repositories;

namespace Stoq.Catalog.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepositoryImpl>();

        return services;
    }

    public static IServiceCollection AddCatalogDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<CatalogModuleDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        return services;
    }
}
