namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record ParsedReceiptItemDto(
    string ProductName,
    decimal Quantity,
    string Unit,
    string? Barcode,
    string? RetailerSku = null,
    decimal? UnitPrice = null,
    decimal? TotalPrice = null,
    decimal? Discount = null,
    Guid CorrectionItemId = default,
    Guid? MatchedProductId = null);
