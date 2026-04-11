using Stoq.Shared.Kernel.Domain;

namespace Stoq.Identity.Domain.Events;

public sealed class HouseholdCreatedDomainEvent(Guid householdId, Guid adminUserId) : DomainEvent
{
    public Guid HouseholdId { get; } = householdId;
    public Guid AdminUserId { get; } = adminUserId;
}
