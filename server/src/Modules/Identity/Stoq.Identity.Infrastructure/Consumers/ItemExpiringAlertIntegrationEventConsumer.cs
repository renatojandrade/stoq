using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Identity.Infrastructure.Consumers;

public sealed class ItemExpiringAlertIntegrationEventConsumer(
    IIdentityRepository identityRepository,
    IDevicePushTokenRepository tokenRepository,
    IExpoPushService pushService,
    ILogger<ItemExpiringAlertIntegrationEventConsumer> logger)
    : IConsumer<ItemExpiringAlertIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ItemExpiringAlertIntegrationEvent> context)
    {
        ItemExpiringAlertIntegrationEvent evt = context.Message;

        IReadOnlyList<Guid> memberIds = await identityRepository
            .GetHouseholdMemberUserIdsAsync(evt.HouseholdId, context.CancellationToken);

        if (memberIds.Count == 0)
        {
            return;
        }

        IReadOnlyList<string> tokens = await tokenRepository
            .GetTokensForUsersAsync(memberIds, context.CancellationToken);

        if (tokens.Count == 0)
        {
            logger.LogDebug(
                "No push tokens found for household {HouseholdId}. Skipping expiry alert.",
                evt.HouseholdId);
            return;
        }

        var body = evt.DaysUntilExpiry == 1
            ? $"{evt.ProductName} expires tomorrow."
            : $"{evt.ProductName} expires in {evt.DaysUntilExpiry} days.";

        var messages = tokens
            .Select(token => new ExpoPushMessage(
                To: token,
                Title: "⏰ Expiry Alert",
                Body: body,
                Data: new Dictionary<string, string> { ["screen"] = "ExpiryAlerts" }))
            .ToList();

        await pushService.SendAsync(messages, context.CancellationToken);

        logger.LogInformation(
            "Sent {Count} expiry alert(s) for product {ProductName} in household {HouseholdId}.",
            messages.Count,
            evt.ProductName,
            evt.HouseholdId);
    }
}
