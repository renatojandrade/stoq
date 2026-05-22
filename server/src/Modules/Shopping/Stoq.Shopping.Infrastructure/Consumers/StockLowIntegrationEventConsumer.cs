using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Shared.Contracts.Events.Inventory;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Domain.Aggregates;
using Stoq.Shopping.Domain.Enums;

namespace Stoq.Shopping.Infrastructure.Consumers;

public sealed class StockLowIntegrationEventConsumer(
    IShoppingListRepository repository,
    ILogger<StockLowIntegrationEventConsumer> logger)
    : IConsumer<StockLowIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StockLowIntegrationEvent> context)
    {
        StockLowIntegrationEvent evt = context.Message;

        logger.LogInformation(
            "Stock low for product {ProductId} in household {HouseholdId}. Auto-adding to shopping list.",
            evt.ProductId,
            evt.HouseholdId);

        IReadOnlyList<ShoppingListItem> existing = await repository
            .ListByHouseholdAsync(evt.HouseholdId, context.CancellationToken);

        var alreadyListed = existing.Any(i =>
            string.Equals(i.ProductName, evt.ProductName, StringComparison.OrdinalIgnoreCase)
            && !i.IsChecked);

        if (alreadyListed)
        {
            return;
        }

        var item = ShoppingListItem.Create(
            evt.HouseholdId,
            evt.ProductName,
            evt.MinQuantity > 0 ? evt.MinQuantity : 1,
            evt.Unit,
            null,
            ShoppingItemSource.Auto);

        await repository.AddAsync(item, context.CancellationToken);
        await repository.SaveChangesAsync(context.CancellationToken);
    }
}
