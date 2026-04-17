using Microsoft.EntityFrameworkCore;
using Stoq.Catalog.Infrastructure.Persistence;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Catalog.Infrastructure.Database;

internal sealed class CatalogDatabaseMigrator(
    CatalogModuleDbContext context,
    CatalogDataSeeder seeder) : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
    }
}
