using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Stoq.ApiService.Hubs;

[Authorize]
public sealed class HouseholdHub : Hub<IHouseholdHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var householdId = Context.User?.FindFirst("householdId")?.Value;
        if (!string.IsNullOrEmpty(householdId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, householdId);
        }

        await base.OnConnectedAsync();
    }
}
