using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Inventory.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.BatchAddInventoryItems;

internal sealed class BatchAddInventoryItemsCommandHandler(
    IInventoryRepository repository,
    IParseCorrectionsRepository correctionsRepository,
    IReceiptSavedPublisher receiptSavedPublisher)
    : IRequestHandler<BatchAddInventoryItemsCommand, Result<IReadOnlyList<InventoryItemDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryItemDto>>> Handle(
        BatchAddInventoryItemsCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<InventoryItemDto>(request.Items.Count);
        var savedItems = new List<(Guid CorrectionItemId, Guid InventoryItemId, string ProductName, string? Barcode, string? RetailerSku)>();

        foreach (BatchInventoryItem item in request.Items)
        {
            var productId = item.MatchedProductId ?? Guid.NewGuid();

            var inventoryItem = InventoryItem.Create(
                request.HouseholdId,
                productId,
                item.ProductName,
                null,
                null,
                item.Quantity,
                0,
                item.Unit,
                item.ExpiryDate,
                item.Notes);

            await repository.AddAsync(inventoryItem, cancellationToken);
            results.Add(ToDto(inventoryItem));

            if (item.CorrectionItemId.HasValue)
            {
                savedItems.Add((
                    item.CorrectionItemId.Value,
                    inventoryItem.Id,
                    item.ProductName,
                    item.Barcode,
                    item.RetailerSku));
            }
        }

        await repository.SaveChangesAsync(cancellationToken);

        if (request.CorrectionId.HasValue)
        {
            ParseCorrection? correction = await correctionsRepository.GetByIdAsync(
                request.CorrectionId.Value,
                cancellationToken);

            if (correction is not null)
            {
                correction.MarkConfirmed();
                await correctionsRepository.SaveChangesAsync(cancellationToken);
            }

            if (savedItems.Count > 0 && request.InitiatedByUserId.HasValue)
            {
                await receiptSavedPublisher.PublishAsync(
                    request.CorrectionId.Value,
                    request.HouseholdId,
                    request.Country,
                    savedItems,
                    request.InitiatedByUserId.Value,
                    cancellationToken);
            }
        }

        return Result<IReadOnlyList<InventoryItemDto>>.Success(results);
    }

    private static InventoryItemDto ToDto(InventoryItem item) =>
        new(
            item.Id,
            item.HouseholdId,
            item.ProductId,
            item.ProductName,
            item.ProductBrand,
            item.ProductImageUrl,
            item.Quantity,
            item.MinQuantity,
            item.Unit,
            item.ExpiryDate,
            item.Notes,
            item.IsStockLow);
}
