using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Queries.ListInventoryItems;

public sealed record ListInventoryItemsQuery(
    Guid HouseholdId,
    string? NameFilter) : IRequest<Result<IReadOnlyList<InventoryItemDto>>>;
