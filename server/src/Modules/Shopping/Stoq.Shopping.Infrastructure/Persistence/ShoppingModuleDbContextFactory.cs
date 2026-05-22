using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stoq.Shopping.Infrastructure.Persistence;

// Used only by EF Core Tools (dotnet ef migrations add …) at design time.
internal sealed class ShoppingModuleDbContextFactory : IDesignTimeDbContextFactory<ShoppingModuleDbContext>
{
    public ShoppingModuleDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<ShoppingModuleDbContext> options = new DbContextOptionsBuilder<ShoppingModuleDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=stoq-shopping;Username=postgres;Password=dev")
            .Options;

        return new ShoppingModuleDbContext(options);
    }
}
