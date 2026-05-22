using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Infrastructure.Services;

namespace Stoq.Inventory.Infrastructure.BackgroundServices;

public sealed class PlaywrightStartupService(
    PlaywrightBrowserService browserService,
    ILogger<PlaywrightStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Warming up Playwright Chromium at startup...");
        await browserService.WarmUpAsync(cancellationToken);
        logger.LogInformation("Playwright Chromium ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
