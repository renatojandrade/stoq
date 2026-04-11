using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.RegenerateInviteCode;

internal sealed class RegenerateInviteCodeCommandHandler(IIdentityRepository repository)
    : IRequestHandler<RegenerateInviteCodeCommand, Result<HouseholdDto>>
{
    public async Task<Result<HouseholdDto>> Handle(RegenerateInviteCodeCommand request, CancellationToken cancellationToken)
    {
        Household? household = await repository
            .GetHouseholdByIdAsync(request.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        if (household is null)
        {
            return Result<HouseholdDto>.NotFound("Household not found.");
        }

        HouseholdMember? requestingMember = household.Members.FirstOrDefault(m => m.UserId == request.RequestingUserId);
        if (requestingMember?.Role != AppRole.HouseholdAdmin)
        {
            return Result<HouseholdDto>.Forbidden("Only admins can regenerate the invite code.");
        }

        household.RegenerateInviteCode();
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<HouseholdDto>.Success(new HouseholdDto(
            household.Id,
            household.Name,
            household.InviteCode.Value,
            household.Members.Count));
    }
}
