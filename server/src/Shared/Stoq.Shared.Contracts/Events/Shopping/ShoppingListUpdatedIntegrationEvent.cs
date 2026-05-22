namespace Stoq.Shared.Contracts.Events.Shopping;

public sealed record ShoppingListUpdatedIntegrationEvent : BaseIntegrationEvent
{
    public ShoppingListUpdatedIntegrationEvent(
        Guid householdId,
        string productName,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        HouseholdId = householdId;
        ProductName = productName;
    }

    public Guid HouseholdId { get; init; }

    public string ProductName { get; init; }
}
