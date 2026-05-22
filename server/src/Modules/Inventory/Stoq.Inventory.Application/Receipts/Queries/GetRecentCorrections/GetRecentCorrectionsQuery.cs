using MediatR;
using Stoq.Inventory.Application.Common.DTOs;

namespace Stoq.Inventory.Application.Receipts.Queries.GetRecentCorrections;

public sealed record GetRecentCorrectionsQuery(Guid HouseholdId, Guid UserId, int Limit = 10)
    : IRequest<IReadOnlyList<ParseCorrectionSummaryDto>>;
