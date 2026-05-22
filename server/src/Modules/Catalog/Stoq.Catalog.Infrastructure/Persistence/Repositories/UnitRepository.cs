using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class UnitRepository(CatalogModuleDbContext context) : IUnitRepository
{
    public async Task<IReadOnlyList<UnitOfMeasure>> ListAllAsync(CancellationToken ct = default) =>
        await context.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.NameEn)
            .ToListAsync(ct);

    public async Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.UnitsOfMeasure
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}
