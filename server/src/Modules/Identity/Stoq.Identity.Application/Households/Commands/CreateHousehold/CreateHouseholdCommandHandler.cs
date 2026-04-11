using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.CreateHousehold;

internal sealed class CreateHouseholdCommandHandler(IIdentityRepository repository)
    : IRequestHandler<CreateHouseholdCommand, Result<HouseholdDto>>
{
    public async Task<Result<HouseholdDto>> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var alreadyMember = await repository
            .IsUserMemberOfAnyHouseholdAsync(request.AdminUserId, cancellationToken);

        if (alreadyMember)
        {
            return Result<HouseholdDto>.Failure("User is already a member of a household.");
        }

        var household = Household.Create(request.Name, request.AdminUserId);

        await repository.AddHouseholdAsync(household, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return Result<HouseholdDto>.Success(new HouseholdDto(
            household.Id,
            household.Name,
            household.InviteCode.Value,
            household.Members.Count));
    }
}
