namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record ParseReceiptResultDto(
    Guid CorrectionId,
    string? RetailerName,
    RetailerInfoDto? RetailerInfo,
    IReadOnlyList<ParsedReceiptItemDto> Items,
    string? ReceiptNumber = null,
    DateTimeOffset? EmittedAt = null);
