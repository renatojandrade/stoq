using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class BrandRepository(CatalogModuleDbContext context) : IBrandRepository
{
    public async Task<Brand?> FindByNameAsync(string name, Guid householdId, CancellationToken ct = default) =>
        await context.Brands
            .AsNoTracking()
            .Where(b => EF.Functions.ILike(b.Name, name) && (b.HouseholdId == householdId || b.HouseholdId == null))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Brand>> SearchAsync(string? nameFilter, Guid householdId, CancellationToken ct = default)
    {
        IQueryable<Brand> query = context.Brands
            .AsNoTracking()
            .Where(b => b.HouseholdId == householdId || b.HouseholdId == null);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{nameFilter}%"));
        }

        return await query
            .OrderBy(b => b.Name)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Brand brand, CancellationToken ct = default) =>
        await context.Brands.AddAsync(brand, ct);
}
