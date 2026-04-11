using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stoq.Identity.Infrastructure.Persistence;
internal sealed class IdentityModuleDbContextFactory : IDesignTimeDbContextFactory<IdentityModuleDbContext>
{
    public IdentityModuleDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<IdentityModuleDbContext> options = new DbContextOptionsBuilder<IdentityModuleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=stoq-identity;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityModuleDbContext.Schema))
            .Options;

        return new IdentityModuleDbContext(options);
    }
}
