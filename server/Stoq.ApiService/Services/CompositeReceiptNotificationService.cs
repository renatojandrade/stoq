using Microsoft.AspNetCore.SignalR;
using Stoq.ApiService.Hubs;
using Stoq.Identity.Application.Ports;
using Stoq.Inventory.Application.Ports;

namespace Stoq.ApiService.Services;

public sealed class CompositeReceiptNotificationService(
    IHubContext<ReceiptHub, IReceiptHubClient> hubContext,
    IDevicePushTokenRepository tokenRepository,
    IExpoPushService pushService)
    : IReceiptNotificationService
{
    public async Task NotifyCorrectionReadyAsync(Guid correctionId, Guid userId, CancellationToken ct)
    {
        await hubContext.Clients.User(userId.ToString()).ReceiptCorrectionReady(correctionId);
        await SendPushAsync(userId, "🧾 Receipt ready", "Tap to review and add items to your inventory.", "ScanList", ct);
    }

    public async Task NotifyCorrectionFailedAsync(Guid correctionId, Guid userId, string reason, CancellationToken ct)
    {
        await hubContext.Clients.User(userId.ToString()).ReceiptCorrectionFailed(correctionId, reason);
        await SendPushAsync(userId, "❌ Receipt scan failed", "Could not read the receipt. Try again or add items manually.", "ScanList", ct);
    }

    private async Task SendPushAsync(Guid userId, string title, string body, string screen, CancellationToken ct)
    {
        IReadOnlyList<string> tokens = await tokenRepository.GetTokensForUsersAsync([userId], ct);

        if (tokens.Count == 0)
        {
            return;
        }

        var messages = tokens
            .Select(token => new ExpoPushMessage(
                To: token,
                Title: title,
                Body: body,
                Data: new Dictionary<string, string> { ["screen"] = screen }))
            .ToList();

        await pushService.SendAsync(messages, ct);
    }
}
