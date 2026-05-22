using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.Receipts.Queries.GetCorrectionDetail;

public sealed record GetCorrectionDetailQuery(Guid CorrectionId, Guid HouseholdId)
    : IRequest<Result<ParseCorrectionDetailDto>>;
