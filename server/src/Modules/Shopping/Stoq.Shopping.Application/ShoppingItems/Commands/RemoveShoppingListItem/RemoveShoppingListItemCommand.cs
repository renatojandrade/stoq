using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.RemoveShoppingListItem;

public sealed record RemoveShoppingListItemCommand(Guid Id, Guid HouseholdId) : IRequest<Result>;
