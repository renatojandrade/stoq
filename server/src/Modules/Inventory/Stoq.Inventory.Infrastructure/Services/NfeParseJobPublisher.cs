using MassTransit;
using Stoq.Inventory.Application.Ports;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Inventory.Infrastructure.Services;

internal sealed class NfeParseJobPublisher(IBus bus) : INfeParseJobPublisher
{
    public Task PublishAsync(
        Guid correctionId,
        string qrCodeUrl,
        Guid householdId,
        Guid initiatedByUserId,
        CancellationToken ct = default) =>
        bus.Publish(
            new NfeParseJobRequestedEvent(correctionId, qrCodeUrl, householdId, initiatedByUserId),
            ct);
}
