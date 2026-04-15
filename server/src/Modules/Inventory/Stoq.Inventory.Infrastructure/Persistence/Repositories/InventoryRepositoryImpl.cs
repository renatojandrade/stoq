using Microsoft.EntityFrameworkCore;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Inventory.Infrastructure.Persistence;

namespace Stoq.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryRepositoryImpl(InventoryModuleDbContext context) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<InventoryItem>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        CancellationToken ct = default)
    {
        IQueryable<InventoryItem> query = context.InventoryItems
            .AsNoTracking()
            .Where(i => i.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(i => EF.Functions.ILike(i.ProductName, $"%{nameFilter}%"));
        }

        return await query
            .OrderBy(i => i.ProductName)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(
        Guid householdId,
        int daysAhead,
        CancellationToken ct = default)
    {
        DateOnly cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));

        return await context.InventoryItems
            .AsNoTracking()
            .Where(i => i.HouseholdId == householdId
                        && i.ExpiryDate.HasValue
                        && i.ExpiryDate.Value <= cutoff)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(InventoryItem item, CancellationToken ct = default) =>
        await context.InventoryItems.AddAsync(item, ct).ConfigureAwait(false);

    public void Remove(InventoryItem item) =>
        context.InventoryItems.Remove(item);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct).ConfigureAwait(false);
}
