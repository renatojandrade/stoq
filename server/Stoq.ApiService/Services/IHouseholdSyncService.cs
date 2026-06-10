namespace Stoq.ApiService.Services;

public interface IHouseholdSyncService
{
    Task BroadcastInventoryUpdatedAsync(Guid householdId, CancellationToken ct = default);

    Task BroadcastShoppingListUpdatedAsync(Guid householdId, CancellationToken ct = default);

    Task BroadcastMemberJoinedAsync(Guid householdId, string displayName, CancellationToken ct = default);
}
