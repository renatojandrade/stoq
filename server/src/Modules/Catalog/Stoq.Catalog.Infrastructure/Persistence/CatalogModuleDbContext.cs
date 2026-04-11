using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Infrastructure.Persistence;

public sealed class CatalogModuleDbContext(DbContextOptions<CatalogModuleDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        // Apply all IEntityTypeConfiguration classes from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogModuleDbContext).Assembly);
    }
}
