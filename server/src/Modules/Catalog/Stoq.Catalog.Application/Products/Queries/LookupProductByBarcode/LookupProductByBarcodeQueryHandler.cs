using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.LookupProductByBarcode;

internal sealed class LookupProductByBarcodeQueryHandler(
    IOpenFoodFactsService openFoodFacts,
    ICosmosProductService cosmos)
    : IRequestHandler<LookupProductByBarcodeQuery, Result<ProductLookupDto>>
{
    // GS1 Brazil prefixes — barcodes starting with 789 or 790 are registered in Brazil.
    private static bool IsBrazilianBarcode(string barcode) =>
        barcode.StartsWith("789", StringComparison.Ordinal)
        || barcode.StartsWith("790", StringComparison.Ordinal);

    public async Task<Result<ProductLookupDto>> Handle(
        LookupProductByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        ProductLookupDto? result;

        if (IsBrazilianBarcode(request.Barcode))
        {
            // Brazilian barcode: Cosmos first (better local coverage), then Open Food Facts.
            result = await cosmos.LookupByBarcodeAsync(request.Barcode, cancellationToken).ConfigureAwait(false)
                     ?? await openFoodFacts.LookupByBarcodeAsync(request.Barcode, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // International barcode: Open Food Facts first, then Cosmos.
            result = await openFoodFacts.LookupByBarcodeAsync(request.Barcode, cancellationToken).ConfigureAwait(false)
                     ?? await cosmos.LookupByBarcodeAsync(request.Barcode, cancellationToken).ConfigureAwait(false);
        }

        if (result is null)
        {
            return Result<ProductLookupDto>.NotFound($"No product found for barcode {request.Barcode}.");
        }

        return Result<ProductLookupDto>.Success(result);
    }
}
