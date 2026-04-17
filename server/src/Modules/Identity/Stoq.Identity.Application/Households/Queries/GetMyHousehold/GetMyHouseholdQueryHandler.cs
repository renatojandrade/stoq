using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Queries.GetMyHousehold;

internal sealed class GetMyHouseholdQueryHandler(IIdentityRepository repository)
    : IRequestHandler<GetMyHouseholdQuery, Result<HouseholdDetailDto>>
{
    public async Task<Result<HouseholdDetailDto>> Handle(GetMyHouseholdQuery request, CancellationToken cancellationToken)
    {
        Household? household = await repository
            .GetHouseholdByUserIdAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (household is null)
        {
            return Result<HouseholdDetailDto>.Failure("User is not a member of any household.");
        }

        IEnumerable<Guid> memberUserIds = household.Members.Select(m => m.UserId);
        Dictionary<Guid, (string DisplayName, string? AvatarUrl)> usersInfo = await repository
            .GetUsersInfoAsync(memberUserIds, cancellationToken)
            .ConfigureAwait(false);

        var members = household.Members
            .Select(m =>
            {
                (var displayName, var avatarUrl) = usersInfo.GetValueOrDefault(m.UserId, ("Unknown", null));
                return new MemberDto(
                    m.UserId,
                    displayName,
                    avatarUrl,
                    m.Role.ToString(),
                    m.JoinedAt,
                    m.UserId == request.UserId);
            })
            .ToList();

        return Result<HouseholdDetailDto>.Success(new HouseholdDetailDto(
            household.Id,
            household.Name,
            household.InviteCode.Value,
            household.InviteCode.ExpiresAt,
            household.InviteCode.IsExpired,
            members,
            household.Country.ToString()));
    }
}
