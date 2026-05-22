using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.ClearCheckedItems;

internal sealed class ClearCheckedItemsCommandHandler(IShoppingListRepository repository)
    : IRequestHandler<ClearCheckedItemsCommand, Result>
{
    public async Task<Result> Handle(
        ClearCheckedItemsCommand request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ShoppingListItem> items = await repository
            .ListByHouseholdAsync(request.HouseholdId, cancellationToken);

        IEnumerable<ShoppingListItem> checked_ = items.Where(i => i.IsChecked);

        repository.RemoveRange(checked_);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
