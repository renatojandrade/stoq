using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;
using Stoq.Shopping.Domain.Enums;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.AddShoppingListItem;

public sealed record AddShoppingListItemCommand(
    Guid HouseholdId,
    string ProductName,
    decimal Quantity,
    string Unit,
    string? Notes,
    ShoppingItemSource Source,
    Guid InitiatedByUserId = default) : IRequest<Result<ShoppingListItemDto>>;
