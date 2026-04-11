using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepositoryImpl(CatalogModuleDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Product>> ListByHouseholdAsync(
        Guid householdId,
        string? nameFilter,
        string? categoryFilter,
        CancellationToken ct = default)
    {
        IQueryable<Product> query = context.Products
            .AsNoTracking()
            .Where(p => p.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{nameFilter}%"));
        }

        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            query = query.Where(p => EF.Functions.ILike(p.Category, categoryFilter));
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await context.Products.AddAsync(product, ct).ConfigureAwait(false);

    public void Remove(Product product) =>
        context.Products.Remove(product);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct).ConfigureAwait(false);
}
