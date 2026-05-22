using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Identity.Infrastructure.Consumers;

public sealed class LowStockDigestIntegrationEventConsumer(
    IIdentityRepository identityRepository,
    IDevicePushTokenRepository tokenRepository,
    IExpoPushService pushService,
    ILogger<LowStockDigestIntegrationEventConsumer> logger)
    : IConsumer<LowStockDigestIntegrationEvent>
{
    public async Task Consume(ConsumeContext<LowStockDigestIntegrationEvent> context)
    {
        LowStockDigestIntegrationEvent evt = context.Message;

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
                "No push tokens for household {HouseholdId}. Skipping low stock digest.",
                evt.HouseholdId);
            return;
        }

        var body = BuildBody(evt.ProductNames);

        var messages = tokens
            .Select(token => new ExpoPushMessage(
                To: token,
                Title: "⚠️ Low stock",
                Body: body,
                Data: new Dictionary<string, string> { ["screen"] = "ShoppingList" }))
            .ToList();

        await pushService.SendAsync(messages, context.CancellationToken);

        logger.LogInformation(
            "Sent {Count} low-stock digest push(es) for household {HouseholdId} ({ProductCount} product(s)).",
            messages.Count,
            evt.HouseholdId,
            evt.ProductNames.Count);
    }

    private static string BuildBody(IReadOnlyList<string> productNames)
    {
        if (productNames.Count == 1)
        {
            return $"{productNames[0]} is running low.";
        }

        if (productNames.Count == 2)
        {
            return $"{productNames[0]} and {productNames[1]} are running low.";
        }

        var others = productNames.Count - 2;
        return $"{productNames[0]}, {productNames[1]} and {others} more item(s) are running low.";
    }
}
