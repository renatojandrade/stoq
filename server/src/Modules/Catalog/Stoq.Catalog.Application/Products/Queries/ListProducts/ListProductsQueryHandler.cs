using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Application.Products.Commands.CreateProduct;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Constants;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListProducts;

internal sealed class ListProductsQueryHandler(
    IProductRepository repository,
    IOpenFoodFactsService openFoodFacts)
    : IRequestHandler<ListProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await repository
            .ListByHouseholdAsync(request.HouseholdId, request.Country, request.Name, request.CategoryId, request.IsReference, request.Page, request.PageSize, cancellationToken);

        // OFFs fallback only on page 1 — subsequent pages query the DB which already has cached results.
        // Skip when filtering for household-only products (OFF results are always reference products).
        if (products.Count == 0 && !string.IsNullOrWhiteSpace(request.Name) && request.Page == 1 && request.IsReference != false)
        {
            IReadOnlyList<ProductLookupDto> offResults = await openFoodFacts
                .SearchByNameAsync(request.Name, request.Country, cancellationToken);

            if (offResults.Count > 0)
            {
                var saved = new List<Product>(offResults.Count);

                foreach (ProductLookupDto dto in offResults)
                {
                    // Skip if we already have this barcode (race between concurrent searches).
                    if (!string.IsNullOrWhiteSpace(dto.Barcode))
                    {
                        var exists = await repository
                            .ExistsByBarcodeAsync(dto.Barcode, cancellationToken);

                        if (exists)
                        {
                            continue;
                        }
                    }

                    var reference = Product.CreateReference(
                        dto.Name,
                        WellKnownIds.Categories.Uncategorized,
                        brandId: null,
                        WellKnownIds.Units.UnitsId,
                        dto.ImageUrl,
                        dto.Barcode,
                        request.Country);

                    await repository.AddAsync(reference, cancellationToken);
                    saved.Add(reference);
                }

                if (saved.Count > 0)
                {
                    await repository.SaveChangesAsync(cancellationToken);
                }

                // Reload saved products with navigation properties.
                var ids = saved.Select(p => p.Id).ToList();
                products = await repository
                    .ListByHouseholdAsync(request.HouseholdId, request.Country, nameFilter: null, categoryIdFilter: null, isReferenceFilter: null, page: 1, pageSize: ids.Count + 1, ct: cancellationToken);

                products = products.Where(p => ids.Contains(p.Id)).ToList().AsReadOnly();
            }
        }

        IReadOnlyList<ProductDto> dtos = products
            .Select(CreateProductCommandHandler.ToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }
}
