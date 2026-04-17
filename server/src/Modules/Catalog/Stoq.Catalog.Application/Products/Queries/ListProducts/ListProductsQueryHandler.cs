using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Enums;
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
            .ListByHouseholdAsync(request.HouseholdId, request.Country, request.Name, request.Category, cancellationToken)
            .ConfigureAwait(false);

        // If a name search returned nothing, fall back to Open Food Facts and auto-save any results
        // as reference products so they appear in future searches without hitting OFFs again.
        if (products.Count == 0 && !string.IsNullOrWhiteSpace(request.Name))
        {
            IReadOnlyList<ProductLookupDto> offResults = await openFoodFacts
                .SearchByNameAsync(request.Name, request.Country, cancellationToken)
                .ConfigureAwait(false);

            if (offResults.Count > 0)
            {
                var saved = new List<Product>(offResults.Count);

                foreach (ProductLookupDto dto in offResults)
                {
                    // Skip if we already have this barcode (race between concurrent searches).
                    if (!string.IsNullOrWhiteSpace(dto.Barcode))
                    {
                        var exists = await repository
                            .ExistsByBarcodeAsync(dto.Barcode, cancellationToken)
                            .ConfigureAwait(false);

                        if (exists)
                        {
                            continue;
                        }
                    }

                    Product reference = Product.CreateReference(
                        dto.Name,
                        dto.Brand,
                        dto.Category ?? "Other",
                        ProductUnit.Units,
                        dto.ImageUrl,
                        dto.Barcode,
                        request.Country);

                    await repository.AddAsync(reference, cancellationToken).ConfigureAwait(false);
                    saved.Add(reference);
                }

                if (saved.Count > 0)
                {
                    await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                products = saved;
            }
        }

        IReadOnlyList<ProductDto> dtos = products
            .Select(p => MapToDto(p))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    private static ProductDto MapToDto(Product p) =>
        new(
            p.Id,
            p.HouseholdId,
            p.Name,
            p.Brand,
            p.Category,
            p.Unit.ToString(),
            p.MinQuantity,
            p.ImageUrl,
            p.Barcode,
            p.Country,
            IsReference: p.HouseholdId is null);
}
