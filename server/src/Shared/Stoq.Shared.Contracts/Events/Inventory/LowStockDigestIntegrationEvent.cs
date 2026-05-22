namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record LowStockDigestIntegrationEvent : BaseIntegrationEvent
{
    public LowStockDigestIntegrationEvent(
        Guid householdId,
        IReadOnlyList<string> productNames,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        HouseholdId = householdId;
        ProductNames = productNames;
    }

    public Guid HouseholdId { get; init; }

    public IReadOnlyList<string> ProductNames { get; init; }
}
