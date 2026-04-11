using Microsoft.EntityFrameworkCore;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Infrastructure.Persistence;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class IdentityRepositoryImpl(IdentityModuleDbContext dbContext)
    : IIdentityRepository
{
    public async Task<Household?> GetHouseholdByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Households
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<Household?> GetHouseholdByInviteCodeAsync(string inviteCode, CancellationToken ct = default) =>
        await dbContext.Households
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.InviteCode.Value == inviteCode, ct)
            .ConfigureAwait(false);

    public async Task<Household?> GetHouseholdByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        Guid? householdId = await dbContext.HouseholdMembers
            .Where(m => m.UserId == userId)
            .Select(m => (Guid?)m.HouseholdId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (householdId is null)
        {
            return null;
        }

        return await GetHouseholdByIdAsync(householdId.Value, ct).ConfigureAwait(false);
    }

    public async Task<Dictionary<Guid, (string DisplayName, string? AvatarUrl)>> GetUsersInfoAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        return await dbContext.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.AvatarUrl })
            .ToDictionaryAsync(u => u.Id, u => (u.DisplayName, u.AvatarUrl), ct)
            .ConfigureAwait(false);
    }

    public async Task AddHouseholdAsync(Household household, CancellationToken ct = default) =>
        await dbContext.Households.AddAsync(household, ct).ConfigureAwait(false);

    public async Task<bool> IsUserMemberOfAnyHouseholdAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.HouseholdMembers
            .AnyAsync(m => m.UserId == userId, ct)
            .ConfigureAwait(false);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
