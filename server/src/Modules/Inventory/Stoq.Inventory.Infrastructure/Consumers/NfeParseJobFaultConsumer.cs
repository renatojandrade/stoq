using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;
using Stoq.Shared.Contracts.Events.Inventory;

namespace Stoq.Inventory.Infrastructure.Consumers;

public sealed class NfeParseJobFaultConsumer(
    IParseCorrectionsRepository repository,
    IReceiptNotificationService notifier,
    ILogger<NfeParseJobFaultConsumer> logger)
    : IConsumer<Fault<NfeParseJobRequestedEvent>>
{
    public async Task Consume(ConsumeContext<Fault<NfeParseJobRequestedEvent>> context)
    {
        NfeParseJobRequestedEvent original = context.Message.Message;
        var reason = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Unknown error";

        logger.LogError(
            "NF-e parse job failed for correction {CorrectionId}: {Reason}",
            original.CorrectionId,
            reason);

        ParseCorrection? correction = await repository.GetByIdAsync(original.CorrectionId, context.CancellationToken);
        if (correction is null)
        {
            logger.LogWarning("ParseCorrection {CorrectionId} not found — cannot mark as failed.", original.CorrectionId);
            return;
        }

        correction.MarkFailed(reason);
        await repository.SaveChangesAsync(context.CancellationToken);

        await notifier.NotifyCorrectionFailedAsync(
            original.CorrectionId,
            original.InitiatedByUserId,
            reason,
            context.CancellationToken);
    }
}
