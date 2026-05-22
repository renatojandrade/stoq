using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Domain;

namespace Stoq.Inventory.Infrastructure.Persistence.Interceptors;

internal sealed class DomainEventDispatchInterceptor(IPublisher publisher)
    : SaveChangesInterceptor
{
    private readonly List<DomainEvent> _pendingEvents = new();

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Collect events before save — deleted entities are detached after SaveChanges
        // and would be invisible to SavedChangesAsync.
        if (eventData.Context is not null)
        {
            var aggregates = eventData.Context.ChangeTracker
                .Entries<AggregateRoot<Guid>>()
                .Select(e => e.Entity)
                .Where(a => a.DomainEvents.Count > 0)
                .ToList();

            _pendingEvents.AddRange(aggregates.SelectMany(a => a.DomainEvents));
            aggregates.ForEach(a => a.ClearDomainEvents());
        }

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var events = _pendingEvents.ToList();
        _pendingEvents.Clear();

        foreach (DomainEvent domainEvent in events)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
