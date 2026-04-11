using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.RegenerateInviteCode;

public sealed record RegenerateInviteCodeCommand(
    Guid HouseholdId,
    Guid RequestingUserId) : IRequest<Result<HouseholdDto>>;
