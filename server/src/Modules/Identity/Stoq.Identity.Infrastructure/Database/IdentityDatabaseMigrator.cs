using Microsoft.EntityFrameworkCore;
using Stoq.Identity.Infrastructure.Persistence;
using Stoq.Shared.Contracts.Database;

namespace Stoq.Identity.Infrastructure.Database;

internal sealed class IdentityDatabaseMigrator(IdentityModuleDbContext context) : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default) =>
        await context.Database.MigrateAsync(cancellationToken);
}
