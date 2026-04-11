namespace Stoq.Identity.Infrastructure.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
    public ApplicationUser User { get; set; } = null!;
}
