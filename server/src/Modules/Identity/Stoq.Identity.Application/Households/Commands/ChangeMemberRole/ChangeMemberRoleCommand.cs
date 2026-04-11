using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(
    Guid HouseholdId,
    Guid TargetUserId,
    string NewRole,
    Guid RequestingUserId) : IRequest<Result>;
