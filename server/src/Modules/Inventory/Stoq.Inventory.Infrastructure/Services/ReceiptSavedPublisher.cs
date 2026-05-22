using MassTransit;
using Stoq.Inventory.Application.Ports;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Inventory.Infrastructure.Services;

internal sealed class ReceiptSavedPublisher(IPublishEndpoint bus) : IReceiptSavedPublisher
{
    public Task PublishAsync(
        Guid correctionId,
        Guid householdId,
        string? country,
        IReadOnlyList<(Guid CorrectionItemId, Guid InventoryItemId, string ProductName, string? Barcode, string? RetailerSku)> items,
        Guid initiatedByUserId,
        CancellationToken ct = default)
    {
        IReadOnlyList<ReceiptItemSavedData> eventItems = items
            .Select(i => new ReceiptItemSavedData(
                i.CorrectionItemId,
                i.InventoryItemId,
                i.ProductName,
                i.Barcode,
                i.RetailerSku))
            .ToList()
            .AsReadOnly();

        return bus.Publish(
            new ReceiptItemsSavedIntegrationEvent(
                correctionId,
                householdId,
                country,
                eventItems,
                initiatedByUserId),
            ct);
    }
}
