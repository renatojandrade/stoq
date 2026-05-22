using MassTransit;
using MediatR;
using Stoq.Inventory.Domain.Events;
using Stoq.Shared.Contracts.Events.Inventory;
using Stoq.Shared.Kernel.Services;

namespace Stoq.Inventory.Infrastructure.EventHandlers;

internal sealed class InventoryItemConsumedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ICurrentUserService currentUserService)
    : INotificationHandler<InventoryItemConsumedDomainEvent>
{
    public async Task Handle(InventoryItemConsumedDomainEvent notification, CancellationToken cancellationToken)
    {
        var isStockLow = notification.MinQuantity > 0 && notification.NewQuantity <= notification.MinQuantity;

        if (!isStockLow)
        {
            return;
        }

        var initiatedBy = currentUserService.UserId ?? Guid.Empty;

        await publishEndpoint.Publish(
            new StockLowIntegrationEvent(
                notification.HouseholdId,
                notification.InventoryItemId,
                notification.ProductId,
                notification.ProductName,
                notification.NewQuantity,
                notification.MinQuantity,
                notification.Unit,
                initiatedBy),
            cancellationToken);
    }
}
