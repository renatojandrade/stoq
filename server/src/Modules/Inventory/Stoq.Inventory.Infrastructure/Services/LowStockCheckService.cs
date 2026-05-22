using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Ports;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Inventory.Infrastructure.Services;

internal sealed class LowStockCheckService(
    IInventoryRepository repository,
    IPublishEndpoint bus,
    ILogger<LowStockCheckService> logger) : ILowStockCheckService
{
    public async Task<int> RunAsync(CancellationToken ct = default)
    {
        IReadOnlyList<LowStockEntry> entries = await repository.GetLowStockItemsAsync(ct);

        if (entries.Count == 0)
        {
            logger.LogInformation("Low stock check: no low-stock items found across all households.");
            return 0;
        }

        var byHousehold = entries
            .GroupBy(e => e.HouseholdId)
            .ToList();

        foreach (var group in byHousehold)
        {
            var productNames = group.Select(e => e.ProductName).ToList();
            var digestEvent = new LowStockDigestIntegrationEvent(
                householdId: group.Key,
                productNames: productNames,
                initiatedByUserId: Guid.Empty);

            await bus.Publish(digestEvent, ct);

            logger.LogInformation(
                "Low stock digest published for household {HouseholdId}: {Count} product(s) low.",
                group.Key,
                productNames.Count);
        }

        return byHousehold.Count;
    }
}
