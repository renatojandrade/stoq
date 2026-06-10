using Microsoft.AspNetCore.SignalR;
using Stoq.ApiService.Hubs;

namespace Stoq.ApiService.Services;

public sealed class HouseholdSyncService(IHubContext<HouseholdHub, IHouseholdHubClient> hubContext)
    : IHouseholdSyncService
{
    public Task BroadcastInventoryUpdatedAsync(Guid householdId, CancellationToken ct = default) =>
        hubContext.Clients.Group(householdId.ToString()).InventoryUpdated(householdId);

    public Task BroadcastShoppingListUpdatedAsync(Guid householdId, CancellationToken ct = default) =>
        hubContext.Clients.Group(householdId.ToString()).ShoppingListUpdated(householdId);

    public Task BroadcastMemberJoinedAsync(Guid householdId, string displayName, CancellationToken ct = default) =>
        hubContext.Clients.Group(householdId.ToString()).MemberJoined(displayName);
}
