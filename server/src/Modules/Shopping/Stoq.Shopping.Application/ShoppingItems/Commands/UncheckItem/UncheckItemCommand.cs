using MediatR;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Application.Common.DTOs;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.UncheckItem;

public sealed record UncheckItemCommand(Guid Id, Guid HouseholdId) : IRequest<Result<ShoppingListItemDto>>;
