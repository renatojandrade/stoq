using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Application.Ports;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListAllAsync(CancellationToken ct = default);
}
