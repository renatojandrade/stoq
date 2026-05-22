using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.ConsumeInventoryItem;

internal sealed class ConsumeInventoryItemCommandHandler(IInventoryRepository repository)
    : IRequestHandler<ConsumeInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(ConsumeInventoryItemCommand request, CancellationToken cancellationToken)
    {
        InventoryItem? item = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null || item.HouseholdId != request.HouseholdId)
        {
            return Result<InventoryItemDto>.NotFound($"Inventory item '{request.Id}' not found.");
        }

        Result consumeResult = item.Consume(request.Amount);

        if (!consumeResult.IsSuccess)
        {
            return Result<InventoryItemDto>.Failure(consumeResult.Error ?? "Consume failed.");
        }

        if (item.Quantity == 0)
        {
            repository.Remove(item);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result<InventoryItemDto>.Success(AddInventoryItemCommandHandler.ToDto(item));
    }
}
