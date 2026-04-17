using Stoq.Shared.Contracts.Database;

namespace Stoq.ApiService.Database;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateModuleDatabasesAsync(
        this IServiceScope scope,
        CancellationToken cancellationToken = default)
    {
        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger("Stoq.ApiService.Database.Migrations");

        IEnumerable<IModuleDatabaseMigrator> migrators = scope.ServiceProvider
            .GetServices<IModuleDatabaseMigrator>();

        foreach (IModuleDatabaseMigrator migrator in migrators)
        {
            try
            {
                await migrator.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database migration failed for module {Migrator}.",
                    migrator.GetType().Name);
            }
        }
    }
}
