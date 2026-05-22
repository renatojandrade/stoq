namespace Stoq.Identity.Domain.Entities;

public sealed class DevicePushToken
{
    // EF Core
    private DevicePushToken()
    {
    }

    private DevicePushToken(Guid id, Guid userId, string token)
    {
        Id = id;
        UserId = userId;
        Token = token;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Expo push token, e.g. "ExponentPushToken[xxxx]".</summary>
    public string Token { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static DevicePushToken Create(Guid userId, string token) =>
        new(Guid.NewGuid(), userId, token);
}
