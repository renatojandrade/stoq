using MediatR;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;
using Stoq.Inventory.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.Receipts.Commands.ParseNfe;

internal sealed class ParseNfeCommandHandler(
    IParseCorrectionsRepository repository,
    INfeParseJobPublisher publisher)
    : IRequestHandler<ParseNfeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ParseNfeCommand request, CancellationToken cancellationToken)
    {
        var pendingCount = await repository.CountByUserAndStatusAsync(
            request.InitiatedByUserId,
            [ParseCorrectionStatus.Pending, ParseCorrectionStatus.Processing],
            cancellationToken);

        if (pendingCount >= request.MaxPendingJobsPerUser)
        {
            return Result<Guid>.TooManyRequests(
                $"You already have {pendingCount} scans in progress. Please wait for them to complete.");
        }

        var correctionId = Guid.NewGuid();
        ParseCorrection correction = ParseCorrection.Create(
            correctionId,
            request.HouseholdId,
            request.QrCodeUrl,
            request.InitiatedByUserId);

        await repository.AddAsync(correction, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            correctionId,
            request.QrCodeUrl,
            request.HouseholdId,
            request.InitiatedByUserId,
            cancellationToken);

        return Result<Guid>.Success(correctionId);
    }
}
