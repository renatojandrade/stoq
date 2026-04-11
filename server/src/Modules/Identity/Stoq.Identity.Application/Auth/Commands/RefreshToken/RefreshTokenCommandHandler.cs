using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private const int _accessTokenLifetimeSeconds = 900;
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = tokenService.GetUserIdFromExpiredToken(request.ExpiredAccessToken);
        if (userId is null)
        {
            return Result<AuthResponse>.Failure("Invalid access token.");
        }

        var valid = await identityService.ValidateRefreshTokenAsync(userId.Value, request.RefreshToken, cancellationToken);
        if (!valid)
        {
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired.");
        }

        UserInfo? user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Failure("User not found.");
        }

        // Rotate: revoke current token, issue a new pair.
        await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        HouseholdMembership membership = await identityService.GetHouseholdMembershipAsync(user.Id, cancellationToken);
        AppRole effectiveRole = membership.HouseholdRole ?? user.Role;

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, effectiveRole, membership.HouseholdId);
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
                membership.HouseholdId)));
    }
}
