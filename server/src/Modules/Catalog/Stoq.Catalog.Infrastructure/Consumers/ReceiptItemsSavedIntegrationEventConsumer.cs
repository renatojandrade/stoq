using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Contracts.Events.Catalog;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Catalog.Infrastructure.Consumers;

public sealed class ReceiptItemsSavedIntegrationEventConsumer(
    IProductRepository repository,
    IPublishEndpoint bus,
    ILogger<ReceiptItemsSavedIntegrationEventConsumer> logger)
    : IConsumer<ReceiptItemsSavedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ReceiptItemsSavedIntegrationEvent> context)
    {
        ReceiptItemsSavedIntegrationEvent evt = context.Message;

        logger.LogDebug(
            "Matching {ItemCount} receipt items against reference catalog for correction {CorrectionId}.",
            evt.Items.Count,
            evt.CorrectionId);

        var results = new List<ProductMatchResultData>(evt.Items.Count);

        foreach (ReceiptItemSavedData item in evt.Items)
        {
            Guid? referenceProductId = null;

            if (!string.IsNullOrWhiteSpace(item.Barcode))
            {
                Product? reference = await repository
                    .GetReferenceProductByBarcodeAsync(item.Barcode, context.CancellationToken);

                referenceProductId = reference?.Id;
            }

            results.Add(new ProductMatchResultData(
                evt.CorrectionId,
                item.CorrectionItemId,
                item.InventoryItemId,
                referenceProductId,
                IsMatched: referenceProductId.HasValue));
        }

        if (results.Count > 0)
        {
            await bus.Publish(
                new ProductsMatchedIntegrationEvent(results, evt.InitiatedByUserId),
                context.CancellationToken);
        }
    }
}
