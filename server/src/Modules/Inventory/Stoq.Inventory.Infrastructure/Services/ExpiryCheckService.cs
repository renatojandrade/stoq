using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Inventory.Infrastructure.Services;

internal sealed class ExpiryCheckService(
    IInventoryRepository repository,
    IPublishEndpoint bus,
    ILogger<ExpiryCheckService> logger) : IExpiryCheckService
{
    private static readonly int[] _alertDays = [1, 3];

    public async Task<int> RunAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var alertCount = 0;

        foreach (var daysUntilExpiry in _alertDays)
        {
            DateOnly targetDate = today.AddDays(daysUntilExpiry);
            IReadOnlyList<InventoryItem> items = await repository.GetItemsExpiringOnDateAsync(targetDate, ct);

            foreach (InventoryItem item in items)
            {
                var alertEvent = new ItemExpiringAlertIntegrationEvent(
                    householdId: item.HouseholdId,
                    inventoryItemId: item.Id,
                    productId: item.ProductId,
                    productName: item.ProductName,
                    expiryDate: item.ExpiryDate!.Value,
                    daysUntilExpiry: daysUntilExpiry,
                    initiatedByUserId: Guid.Empty);

                await bus.Publish(alertEvent, ct);
                alertCount++;
            }

            logger.LogInformation(
                "Expiry check: published {Count} alert(s) for items expiring in {Days} day(s) (target date {Date}).",
                items.Count,
                daysUntilExpiry,
                targetDate);
        }

        return alertCount;
    }
}
