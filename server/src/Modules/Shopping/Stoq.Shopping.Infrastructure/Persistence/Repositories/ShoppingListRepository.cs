using Microsoft.EntityFrameworkCore;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Infrastructure.Persistence.Repositories;

internal sealed class ShoppingListRepository(ShoppingModuleDbContext context) : IShoppingListRepository
{
    public async Task<ShoppingListItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<ShoppingListItem>> ListByHouseholdAsync(
        Guid householdId,
        CancellationToken ct = default) =>
        await context.ShoppingListItems
            .AsNoTracking()
            .Where(i => i.HouseholdId == householdId)
            .OrderBy(i => i.IsChecked)
            .ThenBy(i => i.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(ShoppingListItem item, CancellationToken ct = default) =>
        await context.ShoppingListItems.AddAsync(item, ct);

    public void Remove(ShoppingListItem item) =>
        context.ShoppingListItems.Remove(item);

    public void RemoveRange(IEnumerable<ShoppingListItem> items) =>
        context.ShoppingListItems.RemoveRange(items);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
