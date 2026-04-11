using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Application.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        string? categoryFilter,
        CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken ct = default);
}
