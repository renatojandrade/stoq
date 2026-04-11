using MediatR;

namespace Stoq.Shared.Kernel.Domain;

public abstract class DomainEvent : INotification
{
    protected DomainEvent()
    {
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public DateTimeOffset OccurredAt { get; }
}
