namespace Stoq.Shared.Contracts.Events.Catalog;

public sealed record ProductsMatchedIntegrationEvent : BaseIntegrationEvent
{
    public ProductsMatchedIntegrationEvent(
        IReadOnlyList<ProductMatchResultData> results,
        Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        Results = results;
    }

    public IReadOnlyList<ProductMatchResultData> Results { get; init; }
}
