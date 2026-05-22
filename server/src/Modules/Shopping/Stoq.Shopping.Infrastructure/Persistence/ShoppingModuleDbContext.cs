using Microsoft.EntityFrameworkCore;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Infrastructure.Persistence;

public sealed class ShoppingModuleDbContext(DbContextOptions<ShoppingModuleDbContext> options) : DbContext(options)
{
    public const string Schema = "shopping";

    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShoppingModuleDbContext).Assembly);
    }
}
