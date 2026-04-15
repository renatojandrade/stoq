using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.ConsumeInventoryItem;

public sealed record ConsumeInventoryItemCommand(
    Guid Id,
    Guid HouseholdId,
    decimal Amount) : IRequest<Result<InventoryItemDto>>;
