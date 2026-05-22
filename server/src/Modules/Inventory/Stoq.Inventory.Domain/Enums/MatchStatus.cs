namespace Stoq.Inventory.Domain.Enums;

public enum MatchStatus
{
    Pending = 0,
    MatchedHousehold = 1,
    CreatedAndPending = 2,
    MatchedReference = 3,
    FlaggedUnmatched = 4,
}
