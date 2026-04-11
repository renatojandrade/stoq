using Stoq.Shared.Kernel.Domain;

namespace Stoq.Catalog.Domain.Events;

public sealed class ProductCreatedDomainEvent(Guid productId, Guid householdId) : DomainEvent
{
    public Guid ProductId { get; } = productId;
    public Guid HouseholdId { get; } = householdId;
}
