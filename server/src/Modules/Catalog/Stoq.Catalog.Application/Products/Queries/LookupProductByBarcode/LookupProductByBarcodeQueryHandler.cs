using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.LookupProductByBarcode;

internal sealed class LookupProductByBarcodeQueryHandler(
    IProductRepository repository,
    IOpenFoodFactsService openFoodFacts,
    ICosmosProductService cosmos)
    : IRequestHandler<LookupProductByBarcodeQuery, Result<ProductLookupDto>>
{
    public async Task<Result<ProductLookupDto>> Handle(
        LookupProductByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        // Check our catalog first — avoids duplicate creation and gives the richest data.
        Product? existing = await repository
            .GetByBarcodeAsync(request.Barcode, request.HouseholdId, cancellationToken);

        if (existing is not null)
        {
            return Result<ProductLookupDto>.Success(new ProductLookupDto(
                existing.Barcode!,
                existing.Name,
                existing.Brand?.Name,
                existing.Category?.NameEn,
                existing.ImageUrl,
                ExistingProductId: existing.Id));
        }

        ProductLookupDto? result;

        if (IsBrazilianBarcode(request.Barcode))
        {
            // Brazilian barcode: Cosmos first (better local coverage), then Open Food Facts.
            result = await cosmos.LookupByBarcodeAsync(request.Barcode, cancellationToken)
                     ?? await openFoodFacts.LookupByBarcodeAsync(request.Barcode, cancellationToken);
        }
        else
        {
            // International barcode: Open Food Facts first, then Cosmos.
            result = await openFoodFacts.LookupByBarcodeAsync(request.Barcode, cancellationToken)
                     ?? await cosmos.LookupByBarcodeAsync(request.Barcode, cancellationToken);
        }

        if (result is null)
        {
            return Result<ProductLookupDto>.NotFound($"No product found for barcode {request.Barcode}.");
        }

        return Result<ProductLookupDto>.Success(result);
    }

    // GS1 Brazil prefixes — barcodes starting with 789 or 790 are registered in Brazil.
    private static bool IsBrazilianBarcode(string barcode) =>
        barcode.StartsWith("789", StringComparison.Ordinal)
        || barcode.StartsWith("790", StringComparison.Ordinal);
}
