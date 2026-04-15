using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;

internal sealed class AddInventoryItemCommandHandler(IInventoryRepository repository)
    : IRequestHandler<AddInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(AddInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = InventoryItem.Create(
            request.HouseholdId,
            request.ProductId,
            request.ProductName,
            request.ProductBrand,
            request.ProductImageUrl,
            request.Quantity,
            request.MinQuantity,
            request.Unit,
            request.ExpiryDate,
            request.Notes);

        await repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<InventoryItemDto>.Success(ToDto(item));
    }

    internal static InventoryItemDto ToDto(InventoryItem item) =>
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
