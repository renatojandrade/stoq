using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;

namespace Stoq.Inventory.Application.Receipts.Queries.GetRecentCorrections;

internal sealed class GetRecentCorrectionsQueryHandler(IParseCorrectionsRepository repository)
    : IRequestHandler<GetRecentCorrectionsQuery, IReadOnlyList<ParseCorrectionSummaryDto>>
{
    public async Task<IReadOnlyList<ParseCorrectionSummaryDto>> Handle(
        GetRecentCorrectionsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ParseCorrection> corrections = await repository.GetRecentByUserAsync(
            request.UserId,
            request.Limit,
            cancellationToken);

        return corrections
            .Select(c => new ParseCorrectionSummaryDto(
                c.Id,
                c.Status,
                c.RetailerName,
                c.ReceiptNumber,
                c.EmittedAt,
                c.ParsedItemCount,
                c.CreatedAt,
                c.ParsedAt,
                c.FailureReason))
            .ToList()
            .AsReadOnly();
    }
}
