using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.BatchAddInventoryItems;

public sealed record BatchInventoryItem(
    string ProductName,
    decimal Quantity,
    string Unit,
    DateOnly? ExpiryDate,
    string? Notes,
    Guid? CorrectionItemId = null,
    Guid? MatchedProductId = null,
    string? Barcode = null,
    string? RetailerSku = null);

public sealed record BatchAddInventoryItemsCommand(
    Guid HouseholdId,
    IReadOnlyList<BatchInventoryItem> Items,
    Guid? CorrectionId = null,
    string? Country = null,
    Guid? InitiatedByUserId = null) : IRequest<Result<IReadOnlyList<InventoryItemDto>>>;
