namespace Stoq.Shared.Kernel.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? HouseholdId { get; }
    bool IsAuthenticated { get; }
}
