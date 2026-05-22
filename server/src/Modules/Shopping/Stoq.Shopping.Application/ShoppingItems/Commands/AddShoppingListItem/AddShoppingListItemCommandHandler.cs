using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;
using Stoq.Shopping.Application.Ports;
using Stoq.Shopping.Application.ShoppingItems.Queries.ListShoppingItems;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.AddShoppingListItem;

internal sealed class AddShoppingListItemCommandHandler(
    IShoppingListRepository repository,
    IShoppingEventPublisher eventPublisher)
    : IRequestHandler<AddShoppingListItemCommand, Result<ShoppingListItemDto>>
{
    public async Task<Result<ShoppingListItemDto>> Handle(
        AddShoppingListItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = ShoppingListItem.Create(
            request.HouseholdId,
            request.ProductName,
            request.Quantity,
            request.Unit,
            request.Notes,
            request.Source);

        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishItemAddedAsync(
            request.HouseholdId,
            request.ProductName,
            request.InitiatedByUserId,
            cancellationToken);

        return Result<ShoppingListItemDto>.Success(ListShoppingItemsQueryHandler.ToDto(item));
    }
}
