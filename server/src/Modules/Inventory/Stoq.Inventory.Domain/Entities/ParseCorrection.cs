using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Domain.Entities;

/// <summary>Tracks each NF-e scan job and its lifecycle from Pending through Confirmed or Failed.</summary>
public sealed class ParseCorrection
{
    private readonly List<ParseCorrectionItem> _items = new();

    // EF Core
    private ParseCorrection()
    {
    }

    private ParseCorrection(
        Guid id,
        Guid householdId,
        string qrCodeUrl,
        Guid initiatedByUserId)
    {
        Id = id;
        HouseholdId = householdId;
        QrCodeUrl = qrCodeUrl;
        InitiatedByUserId = initiatedByUserId;
        Status = ParseCorrectionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid HouseholdId { get; private set; }

    public string QrCodeUrl { get; private set; } = string.Empty;

    public ParseCorrectionStatus Status { get; private set; }

    public string? RetailerName { get; private set; }

    public string? RetailerCnpj { get; private set; }

    public string? RetailerTradeName { get; private set; }

    public string? RetailerStreet { get; private set; }

    public string? RetailerCity { get; private set; }

    public string? RetailerState { get; private set; }

    public string? RetailerZipCode { get; private set; }

    public string? ReceiptNumber { get; private set; }

    public DateTimeOffset? EmittedAt { get; private set; }

    public int ParsedItemCount { get; private set; }

    public string? FailureReason { get; private set; }

    public Guid InitiatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ParsedAt { get; private set; }

    public IReadOnlyList<ParseCorrectionItem> Items => _items.AsReadOnly();

    public static ParseCorrection Create(Guid id, Guid householdId, string qrCodeUrl, Guid initiatedByUserId) =>
        new(id, householdId, qrCodeUrl, initiatedByUserId);

    public void MarkProcessing() => Status = ParseCorrectionStatus.Processing;

    public void MarkParsed(
        string? retailerName,
        string? retailerCnpj,
        string? retailerTradeName,
        string? retailerStreet,
        string? retailerCity,
        string? retailerState,
        string? retailerZipCode,
        string? receiptNumber,
        DateTimeOffset? emittedAt,
        int parsedItemCount)
    {
        Status = ParseCorrectionStatus.Parsed;
        RetailerName = retailerName;
        RetailerCnpj = retailerCnpj;
        RetailerTradeName = retailerTradeName;
        RetailerStreet = retailerStreet;
        RetailerCity = retailerCity;
        RetailerState = retailerState;
        RetailerZipCode = retailerZipCode;
        ReceiptNumber = receiptNumber;
        EmittedAt = emittedAt;
        ParsedItemCount = parsedItemCount;
        ParsedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = ParseCorrectionStatus.Failed;
        FailureReason = reason;
    }

    public void MarkConfirmed() => Status = ParseCorrectionStatus.Confirmed;

    /// <summary>Records the raw items extracted from the receipt at parse time.</summary>
    public void SetParsedItems(
        IReadOnlyList<(Guid CorrectionItemId, string ProductName, decimal Quantity, string Unit, string? Barcode, string? RetailerSku, decimal? UnitPrice, decimal? TotalPrice, decimal? Discount, string? Currency)> items)
    {
        _items.Clear();

        foreach ((Guid correctionItemId, string productName, var quantity, string unit, string? barcode, string? retailerSku, decimal? unitPrice, decimal? totalPrice, decimal? discount, string? currency) in items)
        {
            _items.Add(ParseCorrectionItem.Create(
                correctionItemId,
                Id,
                productName,
                quantity,
                unit,
                barcode,
                retailerSku,
                unitPrice,
                totalPrice,
                discount,
                currency));
        }
    }
}
