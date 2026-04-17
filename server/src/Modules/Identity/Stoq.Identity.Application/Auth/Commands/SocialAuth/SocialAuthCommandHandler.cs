using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.SocialAuth;

internal sealed class SocialAuthCommandHandler(
    IIdentityService identityService,
    ISocialAuthService socialAuthService,
    ITokenService tokenService)
    : IRequestHandler<SocialAuthCommand, Result<AuthResponse>>
{
    private const int _accessTokenLifetimeSeconds = 900;
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthResponse>> Handle(SocialAuthCommand request, CancellationToken cancellationToken)
    {
        SocialUserInfo? socialInfo = request.Provider switch
        {
            SocialProvider.Google => await socialAuthService
                .ValidateGoogleTokenAsync(request.ProviderToken, cancellationToken),
            SocialProvider.Facebook => await socialAuthService
                .ValidateFacebookTokenAsync(request.ProviderToken, cancellationToken),
            SocialProvider.Apple => await socialAuthService
                .ValidateAppleTokenAsync(request.ProviderToken, cancellationToken),
            _ => null,
        };

        if (socialInfo is null)
        {
            return Result<AuthResponse>.Failure("Invalid or expired social token.");
        }

        // Find by provider first, then fall back to email (account linking).
        UserInfo? user = await identityService.FindBySocialProviderAsync(
                             request.Provider.ToString(), socialInfo.ProviderId, cancellationToken)
                         ?? await identityService.FindByEmailAsync(socialInfo.Email, cancellationToken);

        if (user is null)
        {
            var displayName = socialInfo.DisplayName ?? request.DisplayName ?? socialInfo.Email.Split('@')[0];
            (var success, var errors) = await identityService.CreateSocialUserAsync(
                socialInfo.Email, displayName, request.Provider.ToString(), socialInfo.ProviderId, cancellationToken);

            if (!success)
            {
                return Result<AuthResponse>.Failure(string.Join(" ", errors));
            }

            user = await identityService.FindBySocialProviderAsync(
                request.Provider.ToString(), socialInfo.ProviderId, cancellationToken);

            if (user is null)
            {
                return Result<AuthResponse>.Failure("Social registration succeeded but user could not be retrieved.");
            }
        }

        HouseholdMembership membership = await identityService.GetHouseholdMembershipAsync(user.Id, cancellationToken);
        AppRole effectiveRole = membership.HouseholdRole ?? user.Role;

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, effectiveRole, membership.HouseholdId, membership.HouseholdCountry);
        var refreshToken = tokenService.GenerateRefreshToken();

        await identityService.StoreRefreshTokenAsync(
            user.Id, refreshToken, DateTimeOffset.UtcNow.Add(_refreshTokenLifetime), cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            _accessTokenLifetimeSeconds,
            new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.AvatarUrl,
                user.PreferredLanguage,
                effectiveRole.ToString(),
                membership.HouseholdId,
                membership.HouseholdCountry?.ToString())));
    }
}
