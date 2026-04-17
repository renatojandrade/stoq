using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.LoginUser;

internal sealed class LoginUserCommandHandler(IIdentityService identityService, ITokenService tokenService)
    : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    private const int _accessTokenLifetimeSeconds = 900;
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        UserInfo? user = await identityService.FindByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        var valid = await identityService.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        if (!valid)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
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
