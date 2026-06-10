namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record InventoryItemDto(
    Guid Id,
    Guid HouseholdId,
    Guid? ProductId,
    string ProductName,
    string? ProductBrand,
    string? ProductImageUrl,
    decimal Quantity,
    decimal MinQuantity,
    string Unit,
    DateOnly? ExpiryDate,
    string? Notes,
    bool IsStockLow);
