using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Ports;

namespace Stoq.Inventory.Infrastructure.BackgroundServices;

internal sealed class ExpiryCheckBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiryCheckBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expiry check background service started.");

        // Run once immediately on startup, then repeat daily.
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunScanAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunScanAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        try
        {
            IExpiryCheckService expiryService = scope.ServiceProvider.GetRequiredService<IExpiryCheckService>();
            var expiryCount = await expiryService.RunAsync(ct);
            logger.LogInformation("Expiry scan completed. {Count} alert event(s) published.", expiryCount);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Expiry scan failed.");
        }

        try
        {
            ILowStockCheckService lowStockService = scope.ServiceProvider.GetRequiredService<ILowStockCheckService>();
            var householdCount = await lowStockService.RunAsync(ct);
            logger.LogInformation("Low stock scan completed. {Count} household digest(s) published.", householdCount);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Low stock scan failed.");
        }
    }
}
