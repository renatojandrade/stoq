namespace Stoq.ApiService.DTOs;

/// <summary>Request body for creating a product.</summary>
public sealed record CreateProductRequest(
    string Name,
    Guid CategoryId,
    string? BrandName,
    Guid UnitId,
    [property: System.Text.Json.Serialization.JsonRequired] decimal MinQuantity,
    string? ImageUrl,
    string? Barcode);

/// <summary>Request body for updating a product.</summary>
public sealed record UpdateProductRequest(
    string Name,
    Guid CategoryId,
    string? BrandName,
    Guid UnitId,
    [property: System.Text.Json.Serialization.JsonRequired] decimal MinQuantity,
    string? ImageUrl,
    string? Barcode);
