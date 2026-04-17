namespace Stoq.Catalog.Application.Common.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid? HouseholdId,
    string Name,
    string? Brand,
    string Category,
    string Unit,
    decimal MinQuantity,
    string? ImageUrl,
    string? Barcode,
    string? Country,
    bool IsReference);
