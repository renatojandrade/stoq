namespace Stoq.Inventory.Application.Ports;

public interface ILowStockCheckService
{
    /// <summary>
    /// Scans all households for products with total quantity at or below their minimum
    /// and publishes one <c>LowStockDigestIntegrationEvent</c> per household.
    /// Returns the number of households notified.
    /// </summary>
    Task<int> RunAsync(CancellationToken ct = default);
}
