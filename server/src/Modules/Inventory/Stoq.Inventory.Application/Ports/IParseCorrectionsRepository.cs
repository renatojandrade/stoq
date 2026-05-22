using Stoq.Inventory.Domain.Entities;
using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Application.Ports;

public interface IParseCorrectionsRepository
{
    Task AddAsync(ParseCorrection correction, CancellationToken ct = default);

    Task<ParseCorrection?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Loads the parent correction that owns the given item ID, enabling status updates via the aggregate.</summary>
    Task<ParseCorrection?> GetByItemIdAsync(Guid correctionItemId, CancellationToken ct = default);

    /// <summary>Returns recent corrections initiated by the given user, newest first.</summary>
    Task<IReadOnlyList<ParseCorrection>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>Counts corrections in the given statuses for the specified user. Used to enforce per-user job limits.</summary>
    Task<int> CountByUserAndStatusAsync(Guid userId, IReadOnlyList<ParseCorrectionStatus> statuses, CancellationToken ct = default);

    /// <summary>
    /// Atomically transitions a correction from Pending or Processing to Processing via a direct SQL update,
    /// bypassing EF Core change tracking. Returns true if the row was found and updated.
    /// </summary>
    Task<bool> MarkProcessingAsync(Guid correctionId, CancellationToken ct = default);

    /// <summary>
    /// Atomically updates all scalar fields on a correction to the Parsed state via a direct SQL update,
    /// bypassing EF Core change tracking to avoid concurrency exceptions from ValueGeneratedOnAdd on Status.
    /// </summary>
    Task MarkParsedAsync(
        Guid correctionId,
        string? retailerName,
        string? retailerCnpj,
        string? retailerTradeName,
        string? retailerStreet,
        string? retailerCity,
        string? retailerState,
        string? retailerZipCode,
        string? receiptNumber,
        DateTimeOffset? emittedAt,
        int parsedItemCount,
        CancellationToken ct = default);

    /// <summary>
    /// Inserts the given items (freshly constructed, never tracked) into the DB.
    /// Uses AddRange + SaveChanges so only INSERT statements are issued — no UPDATE concurrency issues.
    /// </summary>
    Task AddCorrectionItemsAsync(IReadOnlyList<ParseCorrectionItem> items, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recently parsed item names and units for the household,
    /// optionally filtered to a specific retailer. Used to build few-shot examples for Claude.
    /// </summary>
    Task<IReadOnlyList<ParseCorrectionItem>> GetRecentConfirmedItemsAsync(
        Guid householdId,
        string? retailerName,
        int limit,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
