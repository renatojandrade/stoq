using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Queries.GetExpiringItems;

public sealed record GetExpiringItemsQuery(
    Guid HouseholdId,
    int DaysAhead = 7) : IRequest<Result<IReadOnlyList<InventoryItemDto>>>;
