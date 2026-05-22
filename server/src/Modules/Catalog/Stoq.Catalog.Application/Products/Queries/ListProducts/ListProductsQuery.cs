using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListProducts;

public sealed record ListProductsQuery(
    Guid HouseholdId,
    string? Country,
    string? Name,
    Guid? CategoryId,
    bool? IsReference = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<ProductDto>>>;
