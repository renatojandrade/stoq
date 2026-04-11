using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.RemoveMember;

public sealed record RemoveMemberCommand(
    Guid HouseholdId,
    Guid TargetUserId,
    Guid RequestingUserId) : IRequest<Result>;
