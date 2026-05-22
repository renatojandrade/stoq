using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;

namespace Stoq.Shopping.Application.ShoppingItems.Queries.ListShoppingItems;

public sealed record ListShoppingItemsQuery(Guid HouseholdId) : IRequest<Result<IReadOnlyList<ShoppingListItemDto>>>;
