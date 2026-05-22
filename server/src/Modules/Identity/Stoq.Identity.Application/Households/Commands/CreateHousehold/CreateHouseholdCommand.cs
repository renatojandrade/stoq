using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Commands.CreateHousehold;

public sealed record CreateHouseholdCommand(
    string Name,
    Guid AdminUserId,
    HouseholdCountry Country) : IRequest<Result<HouseholdAuthResponse>>;
