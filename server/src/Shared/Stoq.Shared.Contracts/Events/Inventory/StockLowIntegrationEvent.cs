namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record StockLowIntegrationEvent : BaseIntegrationEvent
{
    public StockLowIntegrationEvent(
        Guid householdId,
        Guid inventoryItemId,
        Guid productId,
        string productName,
        decimal currentQuantity,
        decimal minQuantity,
        string unit,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        HouseholdId = householdId;
        InventoryItemId = inventoryItemId;
        ProductId = productId;
        ProductName = productName;
        CurrentQuantity = currentQuantity;
        MinQuantity = minQuantity;
        Unit = unit;
    }

    public Guid HouseholdId { get; init; }
    public Guid InventoryItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public decimal CurrentQuantity { get; init; }
    public decimal MinQuantity { get; init; }
    public string Unit { get; init; }
}
