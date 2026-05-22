using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence;

public sealed class CatalogModuleDbContext(DbContextOptions<CatalogModuleDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        // Apply all IEntityTypeConfiguration classes from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogModuleDbContext).Assembly);
    }
}
