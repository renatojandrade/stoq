using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.ChangeMemberRole;

internal sealed class ChangeMemberRoleCommandHandler(IIdentityRepository repository)
    : IRequestHandler<ChangeMemberRoleCommand, Result>
{
    public async Task<Result> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        Household? household = await repository
            .GetHouseholdByIdAsync(request.HouseholdId, cancellationToken);

        if (household is null)
        {
            return Result.NotFound("Household not found.");
        }

        HouseholdMember? requestingMember = household.Members.FirstOrDefault(m => m.UserId == request.RequestingUserId);
        if (requestingMember?.Role != AppRole.HouseholdAdmin)
        {
            return Result.Forbidden("Only admins can change member roles.");
        }

        if (!Enum.TryParse<AppRole>(request.NewRole, ignoreCase: true, out AppRole newRole)
            || newRole is not (AppRole.HouseholdAdmin or AppRole.HouseholdMember or AppRole.HouseholdViewer))
        {
            return Result.Failure("Invalid role. Must be HouseholdAdmin, HouseholdMember, or HouseholdViewer.");
        }

        Result result = household.ChangeMemberRole(request.TargetUserId, newRole);
        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
