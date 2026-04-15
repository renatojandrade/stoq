using Stoq.Catalog.Application.Common.DTOs;

namespace Stoq.Catalog.Application.Ports;

public interface ICosmosProductService
{
    Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default);
}
