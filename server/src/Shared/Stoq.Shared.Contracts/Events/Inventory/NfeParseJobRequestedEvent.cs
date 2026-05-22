namespace Stoq.Shared.Contracts.Events.Inventory;

public sealed record NfeParseJobRequestedEvent : BaseIntegrationEvent
{
    public NfeParseJobRequestedEvent(Guid correctionId, string qrCodeUrl, Guid householdId, Guid initiatedByUserId)
        : base(initiatedByUserId)
    {
        CorrectionId = correctionId;
        QrCodeUrl = qrCodeUrl;
        HouseholdId = householdId;
    }

    public Guid CorrectionId { get; init; }

    public string QrCodeUrl { get; init; } = string.Empty;

    public Guid HouseholdId { get; init; }
}
