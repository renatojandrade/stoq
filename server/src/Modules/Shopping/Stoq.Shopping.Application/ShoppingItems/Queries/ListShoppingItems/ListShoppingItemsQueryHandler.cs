using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.ShoppingItems.Queries.ListShoppingItems;

internal sealed class ListShoppingItemsQueryHandler(IShoppingListRepository repository)
    : IRequestHandler<ListShoppingItemsQuery, Result<IReadOnlyList<ShoppingListItemDto>>>
{
    public async Task<Result<IReadOnlyList<ShoppingListItemDto>>> Handle(
        ListShoppingItemsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ShoppingListItem> items = await repository
            .ListByHouseholdAsync(request.HouseholdId, cancellationToken);

        IReadOnlyList<ShoppingListItemDto> dtos = items.Select(ToDto).ToList();

        return Result<IReadOnlyList<ShoppingListItemDto>>.Success(dtos);
    }

    internal static ShoppingListItemDto ToDto(ShoppingListItem item) =>
        new(
            item.Id,
            item.HouseholdId,
            item.ProductName,
            item.Quantity,
            item.Unit,
            item.Notes,
            item.IsChecked,
            item.Source.ToString(),
            item.CreatedAt);
}
