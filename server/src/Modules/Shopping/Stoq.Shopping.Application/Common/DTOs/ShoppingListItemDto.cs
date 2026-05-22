namespace Stoq.Shopping.Application.Common.DTOs;

public sealed record ShoppingListItemDto(
    Guid Id,
    Guid HouseholdId,
    string ProductName,
    decimal Quantity,
    string Unit,
    string? Notes,
    bool IsChecked,
    string Source,
    DateTimeOffset CreatedAt);
