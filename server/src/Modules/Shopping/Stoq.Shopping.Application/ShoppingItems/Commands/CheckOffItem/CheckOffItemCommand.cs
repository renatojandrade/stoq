using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.CheckOffItem;

public sealed record CheckOffItemCommand(Guid Id, Guid HouseholdId) : IRequest<Result<ShoppingListItemDto>>;
