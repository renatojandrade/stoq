using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Application.ShoppingItems.Queries.ListShoppingItems;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.UncheckItem;

internal sealed class UncheckItemCommandHandler(IShoppingListRepository repository)
    : IRequestHandler<UncheckItemCommand, Result<ShoppingListItemDto>>
{
    public async Task<Result<ShoppingListItemDto>> Handle(
        UncheckItemCommand request,
        CancellationToken cancellationToken)
    {
        ShoppingListItem? item = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null || item.HouseholdId != request.HouseholdId)
        {
            return Result<ShoppingListItemDto>.NotFound("Shopping list item not found.");
        }

        item.Uncheck();
        await repository.SaveChangesAsync(cancellationToken);

        return Result<ShoppingListItemDto>.Success(ListShoppingItemsQueryHandler.ToDto(item));
    }
}
