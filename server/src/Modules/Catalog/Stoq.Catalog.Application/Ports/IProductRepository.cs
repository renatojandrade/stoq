using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Application.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists products for a household (household-owned + reference products filtered by country).
    /// Returned products include Category, Brand, and Unit navigation properties.
    /// </summary>
    Task<IReadOnlyList<Product>> ListByHouseholdAsync(
        Guid householdId,
        string? country,
        string? nameFilter,
        Guid? categoryIdFilter,
        bool? isReferenceFilter = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken ct = default);

    /// <summary>Finds a product by barcode — household-owned first, then reference products.</summary>
    Task<Product?> GetByBarcodeAsync(string barcode, Guid householdId, CancellationToken ct = default);

    /// <summary>Finds a reference (global) product by barcode. Returns null if only household-owned matches exist.</summary>
    Task<Product?> GetReferenceProductByBarcodeAsync(string barcode, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken ct = default);
}
