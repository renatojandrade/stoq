using System.Text.Json.Serialization;

namespace Stoq.ApiService.DTOs;

/// <summary>Request body for parsing a receipt photo (base64-encoded image).</summary>
public sealed record ParseReceiptPhotoRequest(
    [property: JsonRequired] string ImageBase64,
    string ContentType = "image/jpeg");

/// <summary>Request body for parsing a NF-e receipt via its QR code URL.</summary>
public sealed record ParseNfeReceiptRequest(
    [property: JsonRequired] string QrCodeUrl);

/// <summary>Response body returned immediately when an NF-e parse job is accepted.</summary>
public sealed record NfeJobAcceptedDto(Guid CorrectionId);

/// <summary>Request body for batch-adding items to inventory after receipt confirmation.</summary>
public sealed record BatchAddInventoryItemsRequest(
    [property: JsonRequired] IReadOnlyList<BatchInventoryItemRequest> Items,
    Guid? CorrectionId = null);

/// <summary>A single item in a batch inventory add request.</summary>
public sealed record BatchInventoryItemRequest(
    [property: JsonRequired] string ProductName,
    [property: JsonRequired] decimal Quantity,
    [property: JsonRequired] string Unit,
    DateOnly? ExpiryDate,
    string? Notes,
    Guid? CorrectionItemId = null,
    Guid? MatchedProductId = null,
    string? Barcode = null,
    string? RetailerSku = null);
