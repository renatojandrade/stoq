using System.Reflection;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Infrastructure.BackgroundServices;
using Stoq.Inventory.Infrastructure.Consumers;
using Stoq.Inventory.Infrastructure.Database;
using Stoq.Inventory.Infrastructure.Persistence;
using Stoq.Inventory.Infrastructure.Persistence.Interceptors;
using Stoq.Inventory.Infrastructure.Persistence.Repositories;
using Stoq.Inventory.Infrastructure.Services;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Inventory.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IExpiryCheckService, ExpiryCheckService>();
        services.AddScoped<ILowStockCheckService, LowStockCheckService>();
        services.AddHostedService<ExpiryCheckBackgroundService>();

        services.AddHttpClient<IPhotoReceiptParserService, ClaudeReceiptParserService>();
        services.AddHttpClient<IReceiptTextParserService, ClaudeReceiptTextParserService>();
        services.AddHttpClient<INfeReceiptParserService, NfeReceiptParserService>();

        services.AddSingleton<PlaywrightBrowserService>();
        services.AddHostedService<PlaywrightStartupService>();

        services.Configure<NfeParseOptions>(configuration.GetSection(NfeParseOptions.SectionName));

        services.AddScoped<IParseCorrectionsRepository, ParseCorrectionRepository>();
        services.AddScoped<IReceiptSavedPublisher, ReceiptSavedPublisher>();
        services.AddScoped<INfeParseJobPublisher, NfeParseJobPublisher>();

        return services;
    }

    public static void AddInventoryConsumers(IBusRegistrationConfigurator configurator, int globalPrefetchCount = 10)
    {
        configurator.AddConsumer<NfeParseJobRequestedEventConsumer>()
            .Endpoint(e => e.PrefetchCount = globalPrefetchCount);

        configurator.AddConsumer<NfeParseJobFaultConsumer>();
        configurator.AddConsumer<ProductsMatchedIntegrationEventConsumer>();
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
