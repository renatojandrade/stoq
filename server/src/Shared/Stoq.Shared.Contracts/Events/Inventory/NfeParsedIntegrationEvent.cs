namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record NfeParsedIntegrationEvent : BaseIntegrationEvent
{
    public NfeParsedIntegrationEvent(
        Guid correctionId,
        Guid householdId,
        string? retailerName,
        string? retailerCnpj,
        string? retailerTradeName,
        string? retailerStreet,
        string? retailerCity,
        string? retailerState,
        string? retailerZipCode,
        int parsedItemCount,
        IReadOnlyList<NfeParsedItemData> items,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        CorrectionId = correctionId;
        HouseholdId = householdId;
        RetailerName = retailerName;
        RetailerCnpj = retailerCnpj;
        RetailerTradeName = retailerTradeName;
        RetailerStreet = retailerStreet;
        RetailerCity = retailerCity;
        RetailerState = retailerState;
        RetailerZipCode = retailerZipCode;
        ParsedItemCount = parsedItemCount;
        Items = items;
    }

    public Guid CorrectionId { get; init; }

    public Guid HouseholdId { get; init; }

    public string? RetailerName { get; init; }

    public string? RetailerCnpj { get; init; }

    public string? RetailerTradeName { get; init; }

    public string? RetailerStreet { get; init; }

    public string? RetailerCity { get; init; }

    public string? RetailerState { get; init; }

    public string? RetailerZipCode { get; init; }

    public int ParsedItemCount { get; init; }

    public IReadOnlyList<NfeParsedItemData> Items { get; init; }
}
