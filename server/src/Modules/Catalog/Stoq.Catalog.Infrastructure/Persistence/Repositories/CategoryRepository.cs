using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogModuleDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> ListAllAsync(CancellationToken ct = default) =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .ToListAsync(ct);
}
