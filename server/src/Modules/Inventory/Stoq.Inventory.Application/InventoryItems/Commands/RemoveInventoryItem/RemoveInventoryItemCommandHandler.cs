using MediatR;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.RemoveInventoryItem;

internal sealed class RemoveInventoryItemCommandHandler(IInventoryRepository repository)
    : IRequestHandler<RemoveInventoryItemCommand, Result>
{
    public async Task<Result> Handle(RemoveInventoryItemCommand request, CancellationToken cancellationToken)
    {
        InventoryItem? item = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (item is null || item.HouseholdId != request.HouseholdId)
        {
            return Result.NotFound($"Inventory item '{request.Id}' not found.");
        }

        repository.Remove(item);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
