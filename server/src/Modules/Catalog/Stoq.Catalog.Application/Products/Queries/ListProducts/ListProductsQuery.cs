using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListProducts;

public sealed record ListProductsQuery(
    Guid HouseholdId,
    string? Country,
    string? Name,
    string? Category) : IRequest<Result<IReadOnlyList<ProductDto>>>;
