using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.CreateHousehold;
public sealed record CreateHouseholdCommand(
    string Name,
    Guid AdminUserId) : IRequest<Result<HouseholdDto>>;
