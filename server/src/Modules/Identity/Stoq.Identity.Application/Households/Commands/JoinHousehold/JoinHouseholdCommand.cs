using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.JoinHousehold;
public sealed record JoinHouseholdCommand(
    string InviteCode,
    Guid UserId) : IRequest<Result<HouseholdDto>>;
