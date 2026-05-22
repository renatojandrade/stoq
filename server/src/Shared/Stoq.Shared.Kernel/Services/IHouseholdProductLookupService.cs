namespace Stoq.Shared.Kernel.Services;

/// <summary>
/// Anti-corruption adapter for looking up household-owned products from outside the Catalog module.
/// Implemented by Catalog.Infrastructure; injected wherever cross-module product lookup is needed.
/// </summary>
public interface IHouseholdProductLookupService
{
    /// <summary>Returns the ID of a household-owned product matching the given barcode, or null if none exists.</summary>
    Task<Guid?> FindHouseholdProductByBarcodeAsync(Guid householdId, string barcode, CancellationToken ct = default);
}
