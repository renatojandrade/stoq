namespace Stoq.ApiService.Hubs;

public interface IHouseholdHubClient
{
    Task InventoryUpdated(Guid householdId);

    Task ShoppingListUpdated(Guid householdId);

    Task MemberJoined(string displayName);
}
