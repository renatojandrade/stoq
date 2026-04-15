using Stoq.Inventory.Domain.Aggregates;

namespace Stoq.Inventory.Application.Ports;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryItem>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(
        Guid householdId,
        int daysAhead,
        CancellationToken ct = default);

    Task AddAsync(InventoryItem item, CancellationToken ct = default);

    void Remove(InventoryItem item);

    Task SaveChangesAsync(CancellationToken ct = default);
}
