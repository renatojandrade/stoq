using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListProducts;

internal sealed class ListProductsQueryHandler(IProductRepository repository)
    : IRequestHandler<ListProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await repository
            .ListByHouseholdAsync(request.HouseholdId, request.Name, request.Category, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<ProductDto> dtos = products
            .Select(p => new ProductDto(p.Id, p.HouseholdId, p.Name, p.Brand, p.Category, p.Unit.ToString(), p.MinQuantity, p.ImageUrl, p.Barcode))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }
}
