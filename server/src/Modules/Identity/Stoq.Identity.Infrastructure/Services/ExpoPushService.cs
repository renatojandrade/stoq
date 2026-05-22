using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Stoq.Identity.Application.Ports;

namespace Stoq.Identity.Infrastructure.Services;

public sealed class ExpoPushService(
    HttpClient httpClient,
    ILogger<ExpoPushService> logger)
    : IExpoPushService
{
    private const string _expoPushUrl = "https://exp.host/push/send";

    public async Task SendAsync(IReadOnlyList<ExpoPushMessage> messages, CancellationToken ct = default)
    {
        if (messages.Count == 0)
        {
            return;
        }

        var payload = messages.Select(m => new
        {
            to = m.To,
            title = m.Title,
            body = m.Body,
            data = m.Data,
            sound = "default",
        }).ToList();

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(_expoPushUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Expo Push API returned {StatusCode} for {Count} message(s).",
                response.StatusCode,
                messages.Count);
        }
    }
}
