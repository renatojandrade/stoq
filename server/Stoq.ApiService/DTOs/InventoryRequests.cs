namespace Stoq.ApiService.DTOs;

/// <summary>Request body for adding an inventory item. Product details are snapshotted from the catalog.</summary>
public sealed record AddInventoryItemRequest(
    [property: System.Text.Json.Serialization.JsonRequired] Guid ProductId,
    [property: System.Text.Json.Serialization.JsonRequired] decimal Quantity,
    string Unit,
    DateOnly? ExpiryDate,
    string? Notes);

/// <summary>Request body for consuming (reducing) an inventory item's quantity.</summary>
public sealed record ConsumeInventoryItemRequest(
    [property: System.Text.Json.Serialization.JsonRequired] decimal Amount);
