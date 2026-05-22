using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    Guid RequestingHouseholdId,
    string Name,
    Guid CategoryId,
    string? BrandName,
    Guid UnitId,
    decimal MinQuantity,
    string? ImageUrl,
    string? Barcode) : IRequest<Result>;
