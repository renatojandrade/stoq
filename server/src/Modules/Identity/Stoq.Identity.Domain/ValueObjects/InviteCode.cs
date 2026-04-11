using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Domain;

namespace Stoq.Identity.Domain.ValueObjects;

public sealed class InviteCode : ValueObject
{
    public const int Length = 6;
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private InviteCode(string value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public string Value { get; }

    public DateTimeOffset ExpiresAt { get; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    public static InviteCode Generate()
    {
        var code = string.Create(Length, AllowedChars, (span, chars) =>
        {
            for (var i = 0; i < Length; i++)
            {
                span[i] = chars[Random.Shared.Next(chars.Length)];
            }
        });

        return new InviteCode(code, DateTimeOffset.UtcNow.AddHours(48));
    }

    public static InviteCode Reconstitute(string value, DateTimeOffset expiresAt)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        return new InviteCode(value, expiresAt);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
