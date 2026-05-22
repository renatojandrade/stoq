using Microsoft.EntityFrameworkCore;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;
using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ParseCorrectionRepository(InventoryModuleDbContext context)
    : IParseCorrectionsRepository
{
    public async Task AddAsync(ParseCorrection correction, CancellationToken ct = default) =>
        await context.ParseCorrections.AddAsync(correction, ct);

    public async Task<ParseCorrection?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.ParseCorrections
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<ParseCorrection?> GetByItemIdAsync(Guid correctionItemId, CancellationToken ct = default) =>
        await context.ParseCorrections
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Items.Any(i => i.Id == correctionItemId), ct);

    public async Task<IReadOnlyList<ParseCorrection>> GetRecentByUserAsync(
        Guid userId,
        int limit,
        CancellationToken ct = default) =>
        await context.ParseCorrections
            .AsNoTracking()
            .Where(p => p.InitiatedByUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<int> CountByUserAndStatusAsync(
        Guid userId,
        IReadOnlyList<ParseCorrectionStatus> statuses,
        CancellationToken ct = default) =>
        await context.ParseCorrections
            .AsNoTracking()
            .CountAsync(p => p.InitiatedByUserId == userId && statuses.Contains(p.Status), ct);

    public async Task<IReadOnlyList<ParseCorrectionItem>> GetRecentConfirmedItemsAsync(
        Guid householdId,
        string? retailerName,
        int limit,
        CancellationToken ct = default)
    {
        IQueryable<ParseCorrection> query = context.ParseCorrections
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.HouseholdId == householdId && p.Status == ParseCorrectionStatus.Confirmed && p.Items.Count > 0);

        if (!string.IsNullOrWhiteSpace(retailerName))
        {
            query = query.Where(p => p.RetailerName == retailerName);
        }

        IReadOnlyList<ParseCorrection> corrections = await query
            .OrderByDescending(p => p.ParsedAt)
            .Take(limit)
            .ToListAsync(ct);

        return corrections
            .SelectMany(p => p.Items)
            .ToList();
    }

    public async Task<bool> MarkProcessingAsync(Guid correctionId, CancellationToken ct = default)
    {
        var count = await context.ParseCorrections
            .Where(c => c.Id == correctionId &&
                (c.Status == ParseCorrectionStatus.Pending || c.Status == ParseCorrectionStatus.Processing))
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.Status, ParseCorrectionStatus.Processing),
                ct);

        return count > 0;
    }

    public async Task MarkParsedAsync(
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
        CancellationToken ct = default)
    {
        DateTimeOffset parsedAt = DateTimeOffset.UtcNow;

        await context.ParseCorrections
            .Where(c => c.Id == correctionId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(c => c.Status, ParseCorrectionStatus.Parsed)
                    .SetProperty(c => c.RetailerName, retailerName)
                    .SetProperty(c => c.RetailerCnpj, retailerCnpj)
                    .SetProperty(c => c.RetailerTradeName, retailerTradeName)
                    .SetProperty(c => c.RetailerStreet, retailerStreet)
                    .SetProperty(c => c.RetailerCity, retailerCity)
                    .SetProperty(c => c.RetailerState, retailerState)
                    .SetProperty(c => c.RetailerZipCode, retailerZipCode)
                    .SetProperty(c => c.ReceiptNumber, receiptNumber)
                    .SetProperty(c => c.EmittedAt, emittedAt)
                    .SetProperty(c => c.ParsedItemCount, parsedItemCount)
                    .SetProperty(c => c.ParsedAt, parsedAt),
                ct);
    }

    public async Task AddCorrectionItemsAsync(IReadOnlyList<ParseCorrectionItem> items, CancellationToken ct = default)
    {
        context.Set<ParseCorrectionItem>().AddRange(items);
        await context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
