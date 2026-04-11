namespace Stoq.Shared.Contracts.Events.Identity;

public sealed record HouseholdCreatedIntegrationEvent : BaseIntegrationEvent
{
    public HouseholdCreatedIntegrationEvent(Guid householdId, Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        HouseholdId = householdId;
    }

    public Guid HouseholdId { get; init; }
}
