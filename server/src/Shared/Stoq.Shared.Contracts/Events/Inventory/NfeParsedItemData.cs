namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record NfeParsedItemData(
    Guid CorrectionItemId,
    string ProductName,
    string Unit,
    decimal Quantity,
    string? Barcode,
    string? RetailerSku,
    decimal? UnitPrice,
    decimal? TotalPrice,
    decimal? Discount,
    string? Currency,
    Guid? MatchedProductId);
