namespace Stoq.Identity.Application.Ports;

public interface IExpoPushService
{
    Task SendAsync(IReadOnlyList<ExpoPushMessage> messages, CancellationToken ct = default);
}

public sealed record ExpoPushMessage(
    string To,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string>? Data = null);
