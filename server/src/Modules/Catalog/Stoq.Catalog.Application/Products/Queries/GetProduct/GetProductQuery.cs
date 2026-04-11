using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.GetProduct;

public sealed record GetProductQuery(
    Guid ProductId,
    Guid RequestingHouseholdId) : IRequest<Result<ProductDto>>;
