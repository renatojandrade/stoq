using MassTransit;
using MediatR;
using Stoq.Identity.Domain.Events;
using Stoq.Shared.Contracts.Events.Identity;

namespace Stoq.Identity.Infrastructure.EventHandlers;
internal sealed class HouseholdCreatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<HouseholdCreatedDomainEvent>
{
    public async Task Handle(HouseholdCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new HouseholdCreatedIntegrationEvent(notification.HouseholdId, notification.AdminUserId),
            cancellationToken);
    }
}
