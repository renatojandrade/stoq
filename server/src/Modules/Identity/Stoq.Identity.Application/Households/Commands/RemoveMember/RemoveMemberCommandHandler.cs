using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.RemoveMember;

internal sealed class RemoveMemberCommandHandler(IIdentityRepository repository)
    : IRequestHandler<RemoveMemberCommand, Result>
{
    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        Household? household = await repository
            .GetHouseholdByIdAsync(request.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        if (household is null)
        {
            return Result.NotFound("Household not found.");
        }

        HouseholdMember? requestingMember = household.Members.FirstOrDefault(m => m.UserId == request.RequestingUserId);
        if (requestingMember?.Role != AppRole.HouseholdAdmin)
        {
            return Result.Forbidden("Only admins can remove members.");
        }

        Result result = household.RemoveMember(request.TargetUserId);
        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
