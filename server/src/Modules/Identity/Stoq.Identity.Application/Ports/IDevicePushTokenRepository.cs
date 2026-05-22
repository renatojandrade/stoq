using Stoq.Identity.Domain.Entities;

namespace Stoq.Identity.Application.Ports;

public interface IDevicePushTokenRepository
{
    Task UpsertAsync(DevicePushToken token, CancellationToken ct = default);

    Task RemoveByUserAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetTokensForUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
