using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(CatalogModuleDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> ListByHouseholdAsync(
        Guid householdId,
        string? country,
        string? nameFilter,
        Guid? categoryIdFilter,
        bool? isReferenceFilter = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        // Return household-owned products AND reference products matching the country (or global ones).
        IQueryable<Product> query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .Where(p =>
                p.HouseholdId == householdId
                || (p.HouseholdId == null && (p.Country == null || p.Country == country)));

        if (isReferenceFilter.HasValue)
        {
            query = isReferenceFilter.Value
                ? query.Where(p => p.HouseholdId == null)
                : query.Where(p => p.HouseholdId == householdId);
        }

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{nameFilter}%"));
        }

        if (categoryIdFilter.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryIdFilter.Value);
        }

        var skip = (page - 1) * pageSize;

        return await query
            .OrderBy(p => p.HouseholdId == null ? 1 : 0) // household products first
            .ThenBy(p => p.Name)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken ct = default) =>
        await context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Barcode == barcode, ct);

    public async Task<Product?> GetByBarcodeAsync(string barcode, Guid householdId, CancellationToken ct = default) =>
        await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .AsNoTracking()
            .Where(p => p.Barcode == barcode && (p.HouseholdId == householdId || p.HouseholdId == null))
            .OrderBy(p => p.HouseholdId == null ? 1 : 0) // household-owned first
            .FirstOrDefaultAsync(ct);

    public async Task<Product?> GetReferenceProductByBarcodeAsync(string barcode, CancellationToken ct = default) =>
        await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.HouseholdId == null, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await context.Products.AddAsync(product, ct);

    public void Remove(Product product) =>
        context.Products.Remove(product);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
