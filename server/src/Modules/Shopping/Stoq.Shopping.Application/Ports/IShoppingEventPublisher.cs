namespace Stoq.Shopping.Application.Ports;

public interface IShoppingEventPublisher
{
    Task PublishItemAddedAsync(Guid householdId, string productName, Guid initiatedByUserId, CancellationToken ct = default);
}
