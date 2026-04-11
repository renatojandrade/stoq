using Stoq.Shared.Kernel.Domain;

namespace Stoq.Identity.Domain.Events;
public sealed class MemberJoinedDomainEvent(Guid householdId, Guid userId) : DomainEvent
{
    public Guid HouseholdId { get; } = householdId;
    public Guid UserId { get; } = userId;
}
