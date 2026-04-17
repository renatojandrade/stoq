using Microsoft.EntityFrameworkCore;
using Stoq.Inventory.Infrastructure.Persistence;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Inventory.Infrastructure.Database;

internal sealed class InventoryDatabaseMigrator(InventoryModuleDbContext context) : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default) =>
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
}
