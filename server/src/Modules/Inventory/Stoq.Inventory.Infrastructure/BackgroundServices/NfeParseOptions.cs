namespace Stoq.Inventory.Infrastructure.BackgroundServices;

public sealed class NfeParseOptions
{
    public const string SectionName = "ReceiptParsing";

    public int GlobalPrefetchCount { get; set; } = 10;

    public int MaxPendingJobsPerUser { get; set; } = 5;
}
