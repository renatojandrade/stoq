using Stoq.Shared.Kernel.Domain;

namespace Stoq.Inventory.Domain.Events;

public sealed class InventoryItemConsumedDomainEvent(
    Guid inventoryItemId,
    Guid householdId,
    Guid productId,
    string productName,
    decimal newQuantity,
    decimal minQuantity,
    string unit)
    : DomainEvent
{
    public Guid InventoryItemId { get; } = inventoryItemId;
    public Guid HouseholdId { get; } = householdId;
    public Guid ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public decimal NewQuantity { get; } = newQuantity;
    public decimal MinQuantity { get; } = minQuantity;
    public string Unit { get; } = unit;
}
