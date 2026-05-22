using MassTransit;
using MediatR;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Inventory.Domain.Events;
using Stoq.Shared.Contracts.Events.Inventory;
using Stoq.Shared.Kernel.Services;

namespace Stoq.Inventory.Infrastructure.EventHandlers;

internal sealed class InventoryItemAddedDomainEventHandler(
    IInventoryRepository repository,
    IPublishEndpoint publishEndpoint,
    ICurrentUserService currentUserService)
    : INotificationHandler<InventoryItemAddedDomainEvent>
{
    public async Task Handle(InventoryItemAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        InventoryItem? item = await repository.GetByIdAsync(notification.InventoryItemId, cancellationToken);

        if (item is null || !item.IsStockLow)
        {
            return;
        }

        var initiatedBy = currentUserService.UserId ?? Guid.Empty;

        await publishEndpoint.Publish(
            new StockLowIntegrationEvent(
                item.HouseholdId,
                item.Id,
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.MinQuantity,
                item.Unit,
                initiatedBy),
            cancellationToken);
    }
}
