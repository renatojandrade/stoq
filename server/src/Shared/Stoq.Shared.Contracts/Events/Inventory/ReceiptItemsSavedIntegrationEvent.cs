namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record ReceiptItemsSavedIntegrationEvent : BaseIntegrationEvent
{
    public ReceiptItemsSavedIntegrationEvent(
        Guid correctionId,
        Guid householdId,
        string? country,
        IReadOnlyList<ReceiptItemSavedData> items,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        CorrectionId = correctionId;
        HouseholdId = householdId;
        Country = country;
        Items = items;
    }

    public Guid CorrectionId { get; init; }

    public Guid HouseholdId { get; init; }

    public string? Country { get; init; }

    public IReadOnlyList<ReceiptItemSavedData> Items { get; init; }
}
