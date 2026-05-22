namespace Stoq.ApiService.DTOs;

public sealed record AddShoppingListItemRequest(
    string ProductName,
    decimal Quantity,
    string Unit,
    string? Notes);
