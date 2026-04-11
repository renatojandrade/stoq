using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Contracts.Entities;

namespace Stoq.Identity.Domain.Entities;

public sealed class HouseholdMember : IAuditableEntity
{
    public HouseholdMember(Guid householdId, Guid userId, AppRole role)
    {
        HouseholdId = householdId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    // EF Core
    private HouseholdMember()
    {
    }

    public Guid HouseholdId { get; private set; }
    public Guid UserId { get; private set; }
    public AppRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset CreatedAt { get;  set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    public void ChangeRole(AppRole newRole) => Role = newRole;
}
