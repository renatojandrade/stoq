using Stoq.Shared.Kernel.Domain;

namespace Stoq.Inventory.Domain.Events;

public sealed class InventoryItemAddedDomainEvent(Guid inventoryItemId, Guid householdId) : DomainEvent
{
    public Guid InventoryItemId { get; } = inventoryItemId;
    public Guid HouseholdId { get; } = householdId;
}
