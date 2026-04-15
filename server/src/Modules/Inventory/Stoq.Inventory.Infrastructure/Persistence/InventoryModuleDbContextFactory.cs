using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stoq.Inventory.Infrastructure.Persistence;

/// <summary>
/// Used only by EF Core tools (dotnet ef migrations) at design time.
/// </summary>
internal sealed class InventoryModuleDbContextFactory : IDesignTimeDbContextFactory<InventoryModuleDbContext>
{
    public InventoryModuleDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<InventoryModuleDbContext> options = new DbContextOptionsBuilder<InventoryModuleDbContext>()
            .UseNpgsql("Host=localhost;Database=stoq-inventory;Username=postgres;Password=postgres")
            .Options;

        return new InventoryModuleDbContext(options);
    }
}
