using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Contracts.Events.Shopping;

namespace Stoq.Identity.Infrastructure.Consumers;

public sealed class ShoppingListUpdatedIntegrationEventConsumer(
    IIdentityRepository identityRepository,
    IDevicePushTokenRepository tokenRepository,
    IExpoPushService pushService,
    ILogger<ShoppingListUpdatedIntegrationEventConsumer> logger)
    : IConsumer<ShoppingListUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ShoppingListUpdatedIntegrationEvent> context)
    {
        ShoppingListUpdatedIntegrationEvent evt = context.Message;

        IReadOnlyList<Guid> memberIds = await identityRepository
            .GetHouseholdMemberUserIdsAsync(evt.HouseholdId, context.CancellationToken);

        // Notify everyone except the person who made the change.
        var recipients = memberIds
            .Where(id => id != evt.InitiatedByUserId)
            .ToList();

        if (recipients.Count == 0)
        {
            return;
        }

        IReadOnlyList<string> tokens = await tokenRepository
            .GetTokensForUsersAsync(recipients, context.CancellationToken);

        if (tokens.Count == 0)
        {
            logger.LogDebug(
                "No push tokens for household {HouseholdId} recipients. Skipping shopping list push.",
                evt.HouseholdId);
            return;
        }

        var messages = tokens
            .Select(token => new ExpoPushMessage(
                To: token,
                Title: "🛒 Shopping list",
                Body: $"{evt.ProductName} was added.",
                Data: new Dictionary<string, string> { ["screen"] = "ShoppingList" }))
            .ToList();

        await pushService.SendAsync(messages, context.CancellationToken);

        logger.LogInformation(
            "Sent {Count} shopping list push(es) for household {HouseholdId} (product: {ProductName}).",
            messages.Count,
            evt.HouseholdId,
            evt.ProductName);
    }
}
