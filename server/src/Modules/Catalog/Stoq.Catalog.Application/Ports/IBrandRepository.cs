using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Application.Ports;

public interface IBrandRepository
{
    Task<Brand?> FindByNameAsync(string name, Guid householdId, CancellationToken ct = default);

    Task<IReadOnlyList<Brand>> SearchAsync(string? nameFilter, Guid householdId, CancellationToken ct = default);

    Task AddAsync(Brand brand, CancellationToken ct = default);
}
