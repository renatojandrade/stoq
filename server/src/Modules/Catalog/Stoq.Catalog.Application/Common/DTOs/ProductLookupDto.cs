namespace Stoq.Catalog.Application.Common.DTOs;

public sealed record ProductLookupDto(
    string Barcode,
    string Name,
    string? Brand,
    string? Category,
    string? ImageUrl,
    Guid? ExistingProductId = null);
