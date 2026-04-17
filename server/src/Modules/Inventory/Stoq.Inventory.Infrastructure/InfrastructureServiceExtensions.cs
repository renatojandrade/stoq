using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Infrastructure.Database;
using Stoq.Inventory.Infrastructure.Persistence;
using Stoq.Inventory.Infrastructure.Persistence.Interceptors;
using Stoq.Inventory.Infrastructure.Persistence.Repositories;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Inventory.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepositoryImpl>();

        return services;
    }

    public static IServiceCollection AddInventoryDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<InventoryModuleDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IModuleDatabaseMigrator, InventoryDatabaseMigrator>();

        return services;
    }
}
