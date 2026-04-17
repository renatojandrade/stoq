namespace Stoq.Shared.Contracts.Database;

/// <summary>
/// Implemented by each module's Infrastructure layer to apply its EF Core migrations at startup.
/// Register all implementations with the DI container; <see cref="DatabaseMigrationExtensions"/>
/// will discover and run them in registration order.
/// </summary>
public interface IModuleDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
