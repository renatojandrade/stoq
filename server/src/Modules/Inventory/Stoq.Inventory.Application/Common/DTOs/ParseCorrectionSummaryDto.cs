using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record ParseCorrectionSummaryDto(
    Guid Id,
    ParseCorrectionStatus Status,
    string? RetailerName,
    string? ReceiptNumber,
    DateTimeOffset? EmittedAt,
    int ParsedItemCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ParsedAt,
    string? FailureReason);
