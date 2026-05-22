using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.Receipts.Queries.GetCorrectionDetail;

internal sealed class GetCorrectionDetailQueryHandler(IParseCorrectionsRepository repository)
    : IRequestHandler<GetCorrectionDetailQuery, Result<ParseCorrectionDetailDto>>
{
    public async Task<Result<ParseCorrectionDetailDto>> Handle(
        GetCorrectionDetailQuery request,
        CancellationToken cancellationToken)
    {
        ParseCorrection? correction = await repository.GetByIdAsync(request.CorrectionId, cancellationToken);

        if (correction is null)
        {
            return Result<ParseCorrectionDetailDto>.NotFound($"Correction {request.CorrectionId} not found.");
        }

        if (correction.HouseholdId != request.HouseholdId)
        {
            return Result<ParseCorrectionDetailDto>.Forbidden("Access denied.");
        }

        IReadOnlyList<ParsedReceiptItemDto> items = correction.Items
            .Select(i => new ParsedReceiptItemDto(
                i.ProductName,
                i.Quantity,
                i.Unit,
                i.Barcode,
                i.RetailerSku,
                i.UnitPrice,
                i.TotalPrice,
                i.Discount,
                i.Id,
                MatchedProductId: null))
            .ToList()
            .AsReadOnly();

        return Result<ParseCorrectionDetailDto>.Success(new ParseCorrectionDetailDto(
            correction.Id,
            correction.Status,
            correction.RetailerName,
            correction.ReceiptNumber,
            correction.EmittedAt,
            correction.CreatedAt,
            items));
    }
}
