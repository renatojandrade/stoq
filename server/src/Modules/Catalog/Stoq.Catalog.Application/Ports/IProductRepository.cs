using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Application.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists products for a household (household-owned + reference products filtered by country).
    /// </summary>
    Task<IReadOnlyList<Product>> ListByHouseholdAsync(
        Guid householdId,
        string? country,
        string? nameFilter,
        string? categoryFilter,
        CancellationToken ct = default);

    Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken ct = default);
}
