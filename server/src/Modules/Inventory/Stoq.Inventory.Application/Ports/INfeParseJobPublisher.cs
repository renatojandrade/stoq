namespace Stoq.Inventory.Application.Ports;

public interface INfeParseJobPublisher
{
    Task PublishAsync(Guid correctionId, string qrCodeUrl, Guid householdId, Guid initiatedByUserId, CancellationToken ct = default);
}
