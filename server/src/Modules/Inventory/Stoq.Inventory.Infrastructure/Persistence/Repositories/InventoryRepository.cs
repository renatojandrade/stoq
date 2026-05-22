using Microsoft.EntityFrameworkCore;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Inventory.Infrastructure.Persistence;

namespace Stoq.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryRepository(InventoryModuleDbContext context) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<InventoryItem?> FindBatchAsync(
        Guid householdId,
        Guid productId,
        DateOnly? expiryDate,
        CancellationToken ct = default) =>
        await context.InventoryItems
            .FirstOrDefaultAsync(
                i => i.HouseholdId == householdId
                     && i.ProductId == productId
                     && i.ExpiryDate == expiryDate,
                ct);

    public async Task<IReadOnlyList<InventoryItem>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        Guid? productIdFilter = null,
        CancellationToken ct = default)
    {
        IQueryable<InventoryItem> query = context.InventoryItems
            .AsNoTracking()
            .Where(i => i.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            // Split into words and require each word to match independently.
            // This avoids low phrase-level similarity when word order differs or the user
            // types a partial second word (e.g. "mostarde hei" → "Heinz mostarda amarela").
            // Each word uses trigram similarity (>= 3 chars) for fuzzy/order-independent
            // matching, with an ILike fallback for short terms and exact substring hits.
            var words = nameFilter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var w = word;
                query = w.Length < 3
                    ? query.Where(i => EF.Functions.ILike(i.ProductName, $"%{w}%"))
                    : query.Where(i =>
                        EF.Functions.TrigramsAreWordSimilar(w, i.ProductName)
                        || EF.Functions.ILike(i.ProductName, $"%{w}%"));
            }
        }

        if (productIdFilter.HasValue)
        {
            query = query.Where(i => i.ProductId == productIdFilter.Value);
        }

        return await query
            .OrderBy(i => i.ProductName)
            .ThenBy(i => i.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(
        Guid householdId,
        int daysAhead,
        CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));

        return await context.InventoryItems
            .AsNoTracking()
            .Where(i => i.HouseholdId == householdId
                        && i.ExpiryDate.HasValue
                        && i.ExpiryDate.Value <= cutoff)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetItemsExpiringOnDateAsync(DateOnly date, CancellationToken ct = default) =>
        await context.InventoryItems
            .AsNoTracking()
            .Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value == date)
            .OrderBy(i => i.HouseholdId)
            .ThenBy(i => i.ProductName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LowStockEntry>> GetLowStockItemsAsync(CancellationToken ct = default) =>
        await context.InventoryItems
            .AsNoTracking()
            .GroupBy(i => new { i.HouseholdId, i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.HouseholdId,
                g.Key.ProductName,
                TotalQuantity = g.Sum(i => i.Quantity),
                MinQuantity = g.Max(i => i.MinQuantity),
            })
            .Where(x => x.MinQuantity > 0 && x.TotalQuantity <= x.MinQuantity)
            .OrderBy(x => x.HouseholdId)
            .ThenBy(x => x.ProductName)
            .Select(x => new LowStockEntry(x.HouseholdId, x.ProductName))
            .ToListAsync(ct);

    public async Task AddAsync(InventoryItem item, CancellationToken ct = default) =>
        await context.InventoryItems.AddAsync(item, ct);

    public void Remove(InventoryItem item) =>
        context.InventoryItems.Remove(item);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
