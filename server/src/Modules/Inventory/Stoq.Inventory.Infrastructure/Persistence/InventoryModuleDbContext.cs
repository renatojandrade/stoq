using Microsoft.EntityFrameworkCore;
using Stoq.Inventory.Domain.Aggregates;

namespace Stoq.Inventory.Infrastructure.Persistence;

public sealed class InventoryModuleDbContext(DbContextOptions<InventoryModuleDbContext> options) : DbContext(options)
{
    public const string Schema = "inventory";

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        // Apply all IEntityTypeConfiguration classes from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryModuleDbContext).Assembly);
    }
}
