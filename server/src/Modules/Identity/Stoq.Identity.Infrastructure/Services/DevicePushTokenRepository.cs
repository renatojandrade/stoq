using Microsoft.EntityFrameworkCore;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Infrastructure.Persistence;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class DevicePushTokenRepository(IdentityModuleDbContext context)
    : IDevicePushTokenRepository
{
    public async Task UpsertAsync(DevicePushToken token, CancellationToken ct = default)
    {
        DevicePushToken? existing = await context.DevicePushTokens
            .FirstOrDefaultAsync(t => t.Token == token.Token, ct);

        if (existing is null)
        {
            await context.DevicePushTokens.AddAsync(token, ct);
        }

        // If the token already exists (same device, possibly different user after re-login), leave it as-is.
        // The UserId on the stored token remains valid; the new owner will get the same token next push cycle.
    }

    public async Task RemoveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await context.DevicePushTokens
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetTokensForUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        return await context.DevicePushTokens
            .AsNoTracking()
            .Where(t => ids.Contains(t.UserId))
            .Select(t => t.Token)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
