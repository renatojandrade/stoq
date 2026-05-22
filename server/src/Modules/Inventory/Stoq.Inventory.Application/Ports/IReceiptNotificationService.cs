namespace Stoq.Inventory.Application.Ports;

public interface IReceiptNotificationService
{
    Task NotifyCorrectionReadyAsync(Guid correctionId, Guid userId, CancellationToken ct);

    Task NotifyCorrectionFailedAsync(Guid correctionId, Guid userId, string reason, CancellationToken ct);
}
