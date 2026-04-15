namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record ItemExpiredIntegrationEvent : BaseIntegrationEvent
{
    public ItemExpiredIntegrationEvent(
        Guid householdId,
        Guid inventoryItemId,
        Guid productId,
        string productName,
        DateOnly expiryDate,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        HouseholdId = householdId;
        InventoryItemId = inventoryItemId;
        ProductId = productId;
        ProductName = productName;
        ExpiryDate = expiryDate;
    }

    public Guid HouseholdId { get; init; }
    public Guid InventoryItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public DateOnly ExpiryDate { get; init; }
}
