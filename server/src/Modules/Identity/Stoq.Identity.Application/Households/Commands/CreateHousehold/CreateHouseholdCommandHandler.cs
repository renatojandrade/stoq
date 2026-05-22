using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.CreateHousehold;

internal sealed class CreateHouseholdCommandHandler(
    IIdentityRepository repository,
    IIdentityService identityService,
    ITokenService tokenService)
    : IRequestHandler<CreateHouseholdCommand, Result<HouseholdAuthResponse>>
{
    private const int _accessTokenLifetimeSeconds = 15 * 60;
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<HouseholdAuthResponse>> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var alreadyMember = await repository
            .IsUserMemberOfAnyHouseholdAsync(request.AdminUserId, cancellationToken);

        if (alreadyMember)
        {
            return Result<HouseholdAuthResponse>.Failure("User is already a member of a household.");
        }

        var household = Household.Create(request.Name, request.AdminUserId, request.Country);

        await repository.AddHouseholdAsync(household, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        UserInfo? user = await identityService.FindByIdAsync(request.AdminUserId, cancellationToken);
        if (user is null)
        {
            return Result<HouseholdAuthResponse>.Failure("User not found.");
        }

        var accessToken = tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Role, household.Id, request.Country);
        var refreshToken = tokenService.GenerateRefreshToken();

        await identityService.StoreRefreshTokenAsync(
            user.Id, refreshToken, DateTimeOffset.UtcNow.Add(_refreshTokenLifetime), cancellationToken);

        var householdDto = new HouseholdDto(
            household.Id,
            household.Name,
            household.InviteCode.Value,
            household.Members.Count,
            household.Country.ToString());

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.PreferredLanguage,
            user.Role.ToString(),
            household.Id,
            request.Country.ToString());

        return Result<HouseholdAuthResponse>.Success(new HouseholdAuthResponse(
            accessToken,
            refreshToken,
            _accessTokenLifetimeSeconds,
            userDto,
            householdDto));
    }
}
