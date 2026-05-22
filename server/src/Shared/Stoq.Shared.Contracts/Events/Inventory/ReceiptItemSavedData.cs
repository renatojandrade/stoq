namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record ReceiptItemSavedData(
    Guid CorrectionItemId,
    Guid InventoryItemId,
    string ProductName,
    string? Barcode,
    string? RetailerSku);
