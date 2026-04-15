using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.RemoveInventoryItem;

public sealed record RemoveInventoryItemCommand(Guid Id, Guid HouseholdId) : IRequest<Result>;
