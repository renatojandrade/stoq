using Stoq.Identity.Domain.Enums;

namespace Stoq.Identity.Application.Ports;

public interface IIdentityService
{
    Task<UserInfo?> FindByEmailAsync(string email, CancellationToken ct = default);

    Task<UserInfo?> FindByIdAsync(Guid userId, CancellationToken ct = default);

    Task<UserInfo?> FindBySocialProviderAsync(string provider, string providerId, CancellationToken ct = default);

    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct = default);

    Task<(UserInfo? User, string[] Errors)> CreateUserAsync(
        string email,
        string password,
        string displayName,
        string preferredLanguage,
        CancellationToken ct = default);

    Task<(bool Success, string[] Errors)> CreateSocialUserAsync(
        string email,
        string displayName,
        string provider,
        string providerId,
        CancellationToken ct = default);

    Task<string> GeneratePasswordResetOtpAsync(Guid userId, CancellationToken ct = default);

    Task<bool> ResetPasswordAsync(Guid userId, string otp, string newPassword, CancellationToken ct = default);

    Task StoreRefreshTokenAsync(Guid userId, string token, DateTimeOffset expiry, CancellationToken ct = default);

    Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);

    Task RevokeRefreshTokenAsync(string token, CancellationToken ct = default);

    Task<HouseholdMembership> GetHouseholdMembershipAsync(Guid userId, CancellationToken ct = default);
}

public sealed record UserInfo(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string PreferredLanguage,
    AppRole Role);

public sealed record HouseholdMembership(Guid? HouseholdId, AppRole? HouseholdRole, HouseholdCountry? HouseholdCountry);
