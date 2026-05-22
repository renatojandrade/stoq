namespace Stoq.Inventory.Application.Ports;

public interface IExpiryCheckService
{
    /// <summary>
    /// Scans all households for items expiring in 1 and 3 days and publishes an alert event for each.
    /// Returns the total number of alert events published.
    /// </summary>
    Task<int> RunAsync(CancellationToken ct = default);
}
