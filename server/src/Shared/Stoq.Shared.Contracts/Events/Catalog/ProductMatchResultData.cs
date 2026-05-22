namespace Stoq.Shared.Contracts.Events.Catalog;

public sealed record ProductMatchResultData(
    Guid CorrectionId,
    Guid CorrectionItemId,
    Guid InventoryItemId,
    Guid? MatchedReferenceProductId,
    bool IsMatched);
