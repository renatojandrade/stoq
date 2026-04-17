using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Enums;
using Stoq.Identity.Domain.Events;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IPublisher publisher)
    : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private const int _accessTokenLifetimeSeconds = 900;
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        (UserInfo? user, var errors) = await identityService.CreateUserAsync(
            request.Email, request.Password, request.DisplayName, request.PreferredLanguage, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Failure(string.Join(" ", errors));
        }

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.Role, householdId: null);
        var refreshToken = tokenService.GenerateRefreshToken();

        await identityService.StoreRefreshTokenAsync(
            user.Id, refreshToken, DateTimeOffset.UtcNow.Add(_refreshTokenLifetime), cancellationToken);

        await publisher.Publish(
            new UserRegisteredDomainEvent(user.Id, user.Email, user.DisplayName), cancellationToken);

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
                user.Role.ToString(),
                HouseholdId: null,
                HouseholdCountry: null)));
    }
}
