using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.Ports;

public interface IShoppingListRepository
{
    Task<ShoppingListItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ShoppingListItem>> ListByHouseholdAsync(Guid householdId, CancellationToken ct = default);

    Task AddAsync(ShoppingListItem item, CancellationToken ct = default);

    void Remove(ShoppingListItem item);

    void RemoveRange(IEnumerable<ShoppingListItem> items);

    Task SaveChangesAsync(CancellationToken ct = default);
}
