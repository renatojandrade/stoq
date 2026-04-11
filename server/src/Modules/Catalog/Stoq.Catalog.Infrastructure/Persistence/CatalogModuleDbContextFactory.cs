using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stoq.Catalog.Infrastructure.Persistence;

/// <summary>
/// Used only by EF Core tools (dotnet ef migrations) at design time.
/// </summary>
internal sealed class CatalogModuleDbContextFactory : IDesignTimeDbContextFactory<CatalogModuleDbContext>
{
    public CatalogModuleDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<CatalogModuleDbContext> options = new DbContextOptionsBuilder<CatalogModuleDbContext>()
            .UseNpgsql("Host=localhost;Database=stoq_catalog;Username=postgres;Password=postgres")
            .Options;

        return new CatalogModuleDbContext(options);
    }
}
