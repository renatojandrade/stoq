using Microsoft.AspNetCore.Identity;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Contracts.Entities;

namespace Stoq.Identity.Infrastructure.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public AppRole Role { get; set; } = AppRole.HouseholdMember;
    public string? PasswordResetOtpHash { get; set; }
    public DateTimeOffset? PasswordResetOtpExpiry { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
