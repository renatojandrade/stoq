namespace Stoq.Catalog.Application.Common.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid? HouseholdId,
    string Name,
    Guid CategoryId,
    string CategoryNameEn,
    string CategoryNamePt,
    Guid? BrandId,
    string? BrandName,
    Guid UnitId,
    string UnitCode,
    string UnitNameEn,
    string UnitNamePt,
    decimal MinQuantity,
    string? ImageUrl,
    string? Barcode,
    string? Country,
    bool IsReference);
