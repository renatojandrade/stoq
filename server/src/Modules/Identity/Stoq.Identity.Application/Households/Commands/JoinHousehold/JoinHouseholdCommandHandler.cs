using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.JoinHousehold;

internal sealed class JoinHouseholdCommandHandler(IIdentityRepository repository)
    : IRequestHandler<JoinHouseholdCommand, Result<HouseholdDto>>
{
    public async Task<Result<HouseholdDto>> Handle(JoinHouseholdCommand request, CancellationToken cancellationToken)
    {
        var alreadyMember = await repository
            .IsUserMemberOfAnyHouseholdAsync(request.UserId, cancellationToken);

        if (alreadyMember)
        {
            return Result<HouseholdDto>.Failure("User is already a member of a household.");
        }

        Household? household = await repository
            .GetHouseholdByInviteCodeAsync(request.InviteCode, cancellationToken);

        if (household is null)
        {
            return Result<HouseholdDto>.Failure("Invalid invite code.");
        }

        if (household.InviteCode.IsExpired)
        {
            return Result<HouseholdDto>.Failure("Invite code has expired.");
        }

        Result result = household.AddMember(request.UserId);

        if (result.IsFailure)
        {
            return Result<HouseholdDto>.Failure(result.Error);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result<HouseholdDto>.Success(new HouseholdDto(
            household.Id,
            household.Name,
            household.InviteCode.Value,
            household.Members.Count,
            household.Country.ToString()));
    }
}
