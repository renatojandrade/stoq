using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Domain;

namespace Stoq.Inventory.Infrastructure.Persistence.Interceptors;

internal sealed class DomainEventDispatchInterceptor(IPublisher publisher)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (DomainEvent? domainEvent in events)
        {
            await publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}
