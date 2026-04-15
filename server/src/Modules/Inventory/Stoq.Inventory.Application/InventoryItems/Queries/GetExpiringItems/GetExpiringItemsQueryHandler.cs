using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Queries.GetExpiringItems;

internal sealed class GetExpiringItemsQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetExpiringItemsQuery, Result<IReadOnlyList<InventoryItemDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryItemDto>>> Handle(
        GetExpiringItemsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<InventoryItem> items = await repository
            .GetExpiringAsync(request.HouseholdId, request.DaysAhead, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<InventoryItemDto> dtos = items.Select(AddInventoryItemCommandHandler.ToDto).ToList();

        return Result<IReadOnlyList<InventoryItemDto>>.Success(dtos);
    }
}
