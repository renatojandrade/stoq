using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.RemoveShoppingListItem;

internal sealed class RemoveShoppingListItemCommandHandler(IShoppingListRepository repository)
    : IRequestHandler<RemoveShoppingListItemCommand, Result>
{
    public async Task<Result> Handle(
        RemoveShoppingListItemCommand request,
        CancellationToken cancellationToken)
    {
        ShoppingListItem? item = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null || item.HouseholdId != request.HouseholdId)
        {
            return Result.NotFound("Shopping list item not found.");
        }

        repository.Remove(item);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
