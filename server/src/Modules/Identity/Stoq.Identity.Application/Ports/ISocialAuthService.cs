namespace Stoq.Identity.Application.Ports;

public interface ISocialAuthService
{
    Task<SocialUserInfo?> ValidateGoogleTokenAsync(string idToken, CancellationToken ct = default);

    Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken, CancellationToken ct = default);

    Task<SocialUserInfo?> ValidateAppleTokenAsync(string identityToken, CancellationToken ct = default);
}

public sealed record SocialUserInfo(string ProviderId, string Email, string? DisplayName, string? AvatarUrl);
