namespace Stoq.Shared.Contracts.Events;

public abstract record BaseIntegrationEvent
{
    protected BaseIntegrationEvent(Guid initiatedByUserId)
    {
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTimeOffset.UtcNow;
        InitiatedByUserId = initiatedByUserId;
    }

    public Guid CorrelationId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public Guid InitiatedByUserId { get; init; }
}
