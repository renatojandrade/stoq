using MassTransit;
using Stoq.Shopping.Application.Ports;
using Stoq.Shared.Contracts.Events.Shopping;

namespace Stoq.Shopping.Infrastructure.Services;

internal sealed class ShoppingEventPublisher(IPublishEndpoint bus) : IShoppingEventPublisher
{
    public Task PublishItemAddedAsync(
        Guid householdId,
        string productName,
        Guid initiatedByUserId,
        CancellationToken ct = default)
    {
        var evt = new ShoppingListUpdatedIntegrationEvent(householdId, productName, initiatedByUserId);
        return bus.Publish(evt, ct);
    }
}
