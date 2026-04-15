using MediatR;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;

public sealed record AddInventoryItemCommand(
    Guid HouseholdId,
    Guid ProductId,
    string ProductName,
    string? ProductBrand,
    string? ProductImageUrl,
    decimal Quantity,
    decimal MinQuantity,
    string Unit,
    DateOnly? ExpiryDate,
    string? Notes) : IRequest<Result<InventoryItemDto>>;
