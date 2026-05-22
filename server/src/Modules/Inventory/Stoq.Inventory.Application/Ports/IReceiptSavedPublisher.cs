namespace Stoq.Inventory.Application.Ports;

public interface IReceiptSavedPublisher
{
    Task PublishAsync(
        Guid correctionId,
        Guid householdId,
        string? country,
        IReadOnlyList<(Guid CorrectionItemId, Guid InventoryItemId, string ProductName, string? Barcode, string? RetailerSku)> items,
        Guid initiatedByUserId,
        CancellationToken ct = default);
}
