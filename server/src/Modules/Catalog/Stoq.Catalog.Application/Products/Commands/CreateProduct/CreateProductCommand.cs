using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid HouseholdId,
    string Name,
    Guid CategoryId,
    string? BrandName,
    Guid UnitId,
    decimal MinQuantity,
    string? ImageUrl,
    string? Barcode) : IRequest<Result<ProductDto>>;
