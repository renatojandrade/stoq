using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Application.Ports;

public interface IUnitRepository
{
    Task<IReadOnlyList<UnitOfMeasure>> ListAllAsync(CancellationToken ct = default);

    Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
