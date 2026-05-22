using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Domain.Entities;

public sealed class ParseCorrectionItem
{
    // EF Core
    private ParseCorrectionItem()
    {
    }

    private ParseCorrectionItem(
        Guid id,
        Guid parseCorrectionId,
        string productName,
        decimal quantity,
        string unit,
        string? barcode,
        string? retailerSku,
        decimal? unitPrice,
        decimal? totalPrice,
        decimal? discount,
        string? currency)
    {
        Id = id;
        ParseCorrectionId = parseCorrectionId;
        ProductName = productName;
        Quantity = quantity;
        Unit = unit;
        Barcode = barcode;
        RetailerSku = retailerSku;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
        Discount = discount;
        Currency = currency;
        MatchStatus = MatchStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid ParseCorrectionId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Barcode { get; private set; }

    public string? RetailerSku { get; private set; }

    public decimal? UnitPrice { get; private set; }

    public decimal? TotalPrice { get; private set; }

    public decimal? Discount { get; private set; }

    public string? Currency { get; private set; }

    public Guid? MatchedReferenceProductId { get; private set; }

    public MatchStatus MatchStatus { get; private set; }

    public static ParseCorrectionItem Create(
        Guid id,
        Guid parseCorrectionId,
        string productName,
        decimal quantity,
        string unit,
        string? barcode = null,
        string? retailerSku = null,
        decimal? unitPrice = null,
        decimal? totalPrice = null,
        decimal? discount = null,
        string? currency = null) =>
        new(
            id,
            parseCorrectionId,
            productName,
            quantity,
            unit,
            barcode,
            retailerSku,
            unitPrice,
            totalPrice,
            discount,
            currency);

    public void UpdateMatchStatus(MatchStatus status, Guid? referenceProductId = null)
    {
        MatchStatus = status;

        if (referenceProductId.HasValue)
        {
            MatchedReferenceProductId = referenceProductId;
        }
    }
}
