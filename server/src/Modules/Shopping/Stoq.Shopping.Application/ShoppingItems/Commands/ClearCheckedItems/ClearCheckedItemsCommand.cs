using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.ClearCheckedItems;

public sealed record ClearCheckedItemsCommand(Guid HouseholdId) : IRequest<Result>;
