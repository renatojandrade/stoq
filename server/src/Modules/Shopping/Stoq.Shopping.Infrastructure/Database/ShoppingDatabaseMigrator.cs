using Microsoft.EntityFrameworkCore;
using Stoq.Shared.Contracts.Database;
using Stoq.Shopping.Infrastructure.Persistence;

namespace Stoq.Shopping.Infrastructure.Database;

internal sealed class ShoppingDatabaseMigrator(ShoppingModuleDbContext context) : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default) =>
        await context.Database.MigrateAsync(cancellationToken);
}
