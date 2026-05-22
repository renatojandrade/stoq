using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Shared.Contracts.Database;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Infrastructure.Consumers;
using Stoq.Shopping.Infrastructure.Database;
using Stoq.Shopping.Infrastructure.Persistence;
using Stoq.Shopping.Infrastructure.Persistence.Interceptors;
using Stoq.Shopping.Infrastructure.Persistence.Repositories;
using Stoq.Shopping.Infrastructure.Services;

namespace Stoq.Shopping.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddShoppingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.AddScoped<IShoppingEventPublisher, ShoppingEventPublisher>();

        return services;
    }

    public static IServiceCollection AddShoppingDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ShoppingModuleDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IModuleDatabaseMigrator, ShoppingDatabaseMigrator>();

        return services;
    }

    public static void AddShoppingConsumers(IBusRegistrationConfigurator configurator) =>
        configurator.AddConsumer<StockLowIntegrationEventConsumer>();
}
