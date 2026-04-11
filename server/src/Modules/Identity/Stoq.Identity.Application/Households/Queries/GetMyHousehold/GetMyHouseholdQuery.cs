using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Households.Queries.GetMyHousehold;

public sealed record GetMyHouseholdQuery(Guid UserId) : IRequest<Result<HouseholdDetailDto>>;
