using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Application.Products.Commands.CreateProduct;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.GetProduct;

internal sealed class GetProductQueryHandler(IProductRepository repository)
    : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(request.ProductId, cancellationToken);

        // Allow fetching household-owned products or reference products visible to the household.
        if (product is null || (product.HouseholdId is not null && product.HouseholdId != request.RequestingHouseholdId))
        {
            return Result<ProductDto>.NotFound("Product not found.");
        }

        return Result<ProductDto>.Success(CreateProductCommandHandler.ToDto(product));
    }
}
