using Stoq.Catalog.Application.Common.DTOs;

namespace Stoq.Catalog.Application.Ports;

public interface IOpenFoodFactsService
{
    Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default);

    Task<IReadOnlyList<ProductLookupDto>> SearchByNameAsync(string name, string? country, CancellationToken ct = default);
}
