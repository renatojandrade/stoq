using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Kernel.Services;

namespace Stoq.Catalog.Infrastructure.Services;

internal sealed class HouseholdProductLookupService(IProductRepository repository)
    : IHouseholdProductLookupService
{
    public async Task<Guid?> FindHouseholdProductByBarcodeAsync(
        Guid householdId,
        string barcode,
        CancellationToken ct = default)
    {
        Product? product = await repository.GetByBarcodeAsync(barcode, householdId, ct);

        if (product is null || product.HouseholdId != householdId)
        {
            return null;
        }

        return product.Id;
    }
}
