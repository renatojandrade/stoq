using Stoq.Inventory.Domain.Aggregates;

namespace Stoq.Inventory.Application.Ports;

public sealed record LowStockEntry(Guid HouseholdId, string ProductName);

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<InventoryItem?> FindBatchAsync(Guid householdId, Guid productId, DateOnly? expiryDate, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryItem>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        Guid? productIdFilter = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(
        Guid householdId,
        int daysAhead,
        CancellationToken ct = default);

    /// <summary>Returns all items across all households expiring on the given date (used by the background scanner).</summary>
    Task<IReadOnlyList<InventoryItem>> GetItemsExpiringOnDateAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Returns one entry per low-stock product per household (total batch quantity &lt;= minQuantity).
    /// Used by the daily digest background job.
    /// </summary>
    Task<IReadOnlyList<LowStockEntry>> GetLowStockItemsAsync(CancellationToken ct = default);

    Task AddAsync(InventoryItem item, CancellationToken ct = default);

    void Remove(InventoryItem item);

    Task SaveChangesAsync(CancellationToken ct = default);
}
