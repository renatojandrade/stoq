namespace Stoq.Inventory.Domain.Enums;

public enum ParseCorrectionStatus
{
    Pending = 0,
    Processing = 1,
    Parsed = 2,
    Confirmed = 3,
    Failed = 4,
}
