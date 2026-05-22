using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record ParseCorrectionDetailDto(
    Guid Id,
    ParseCorrectionStatus Status,
    string? RetailerName,
    string? ReceiptNumber,
    DateTimeOffset? EmittedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ParsedReceiptItemDto> Items);
