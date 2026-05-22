namespace Stoq.Inventory.Infrastructure.BackgroundServices;

internal sealed record NfeParseJob(
    Guid JobId,
    string QrCodeUrl,
    Guid HouseholdId,
    Guid UserId);
