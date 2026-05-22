using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Queries.ListInventoryItems;

internal sealed class ListInventoryItemsQueryHandler(IInventoryRepository repository)
    : IRequestHandler<ListInventoryItemsQuery, Result<IReadOnlyList<InventoryItemDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryItemDto>>> Handle(
        ListInventoryItemsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<InventoryItem> items = await repository
            .ListByHouseholdAsync(request.HouseholdId, request.NameFilter, request.ProductIdFilter, cancellationToken);

        IReadOnlyList<InventoryItemDto> dtos = items.Select(AddInventoryItemCommandHandler.ToDto).ToList();

        return Result<IReadOnlyList<InventoryItemDto>>.Success(dtos);
    }
}
