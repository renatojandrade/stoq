using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Inventory.Domain.Entities;
using Stoq.Inventory.Domain.Enums;
using Stoq.Shared.Contracts.Events.Catalog;

namespace Stoq.Inventory.Infrastructure.Consumers;

public sealed class ProductsMatchedIntegrationEventConsumer(
    IParseCorrectionsRepository repository,
    IInventoryRepository inventoryRepository,
    ILogger<ProductsMatchedIntegrationEventConsumer> logger)
    : IConsumer<ProductsMatchedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductsMatchedIntegrationEvent> context)
    {
        ProductsMatchedIntegrationEvent evt = context.Message;

        logger.LogDebug(
            "Processing {ResultCount} product match results.",
            evt.Results.Count);

        foreach (ProductMatchResultData result in evt.Results)
        {
            ParseCorrection? correction = await repository
                .GetByItemIdAsync(result.CorrectionItemId, context.CancellationToken);

            if (correction is null)
            {
                logger.LogWarning(
                    "ParseCorrection not found for item {CorrectionItemId}.",
                    result.CorrectionItemId);

                continue;
            }

            ParseCorrectionItem? item = correction.Items
                .FirstOrDefault(i => i.Id == result.CorrectionItemId);

            if (item is null)
            {
                continue;
            }

            MatchStatus newStatus = result.IsMatched
                ? MatchStatus.MatchedReference
                : MatchStatus.FlaggedUnmatched;

            item.UpdateMatchStatus(newStatus, result.MatchedReferenceProductId);

            if (result.IsMatched && result.MatchedReferenceProductId.HasValue)
            {
                InventoryItem? inventoryItem = await inventoryRepository
                    .GetByIdAsync(result.InventoryItemId, context.CancellationToken);

                inventoryItem?.LinkToProduct(result.MatchedReferenceProductId.Value);
            }
        }

        await repository.SaveChangesAsync(context.CancellationToken);
    }
}
