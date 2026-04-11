using Stoq.Identity.Domain.Aggregates;

namespace Stoq.Identity.Application.Ports;

public interface IIdentityRepository
{
    Task<Household?> GetHouseholdByIdAsync(Guid id, CancellationToken ct = default);
    Task<Household?> GetHouseholdByInviteCodeAsync(string inviteCode, CancellationToken ct = default);
    Task<Household?> GetHouseholdByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Dictionary<Guid, (string DisplayName, string? AvatarUrl)>> GetUsersInfoAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task AddHouseholdAsync(Household household, CancellationToken ct = default);
    Task<bool> IsUserMemberOfAnyHouseholdAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
