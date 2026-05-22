using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;
using Stoq.Identity.Domain.Events;
using Stoq.Identity.Domain.ValueObjects;
using Stoq.Shared.Contracts.Entities;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Domain.Aggregates;

public sealed class Household : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<HouseholdMember> _members = [];

    // EF Core
    private Household()
    {
    }

    private Household(
        Guid id,
        string name,
        Guid adminUserId,
        HouseholdCountry country)
        : base(id)
    {
        Name = name;
        Country = country;
        InviteCode = InviteCode.Generate();

        var adminMember = new HouseholdMember(id, adminUserId, AppRole.HouseholdAdmin);
        _members.Add(adminMember);

        RaiseDomainEvent(new HouseholdCreatedDomainEvent(id, adminUserId));
    }

    public string Name { get; private set; } = string.Empty;

    public HouseholdCountry Country { get; private set; }

    public InviteCode InviteCode { get; private set; } = null!;

    public Guid? DefaultDeliveryAddressId { get; private set; }

    // Audit columns (IAuditableEntity).
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid UpdatedBy { get; set; }

    public IReadOnlyList<HouseholdMember> Members => _members.AsReadOnly();

    public static Household Create(string name, Guid adminUserId, HouseholdCountry country)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstEmptyGuid(adminUserId, nameof(adminUserId));

        return new Household(Guid.NewGuid(), name, adminUserId, country);
    }

    public Result AddMember(Guid userId, AppRole role = AppRole.HouseholdMember)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            return Result.Failure("User is already a member of this household.");
        }

        _members.Add(new HouseholdMember(Id, userId, role));
        RaiseDomainEvent(new MemberJoinedDomainEvent(Id, userId));

        return Result.Success();
    }

    public Result RemoveMember(Guid userId)
    {
        HouseholdMember? member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            return Result.Failure("User is not a member of this household.");
        }

        if (member.Role == AppRole.HouseholdAdmin && _members.Count(m => m.Role == AppRole.HouseholdAdmin) == 1)
        {
            return Result.Failure("Cannot remove the last Admin from the household.");
        }

        _members.Remove(member);
        return Result.Success();
    }

    public Result ChangeMemberRole(Guid targetUserId, AppRole newRole)
    {
        HouseholdMember? target = _members.FirstOrDefault(m => m.UserId == targetUserId);
        if (target is null)
        {
            return Result.Failure("User is not a member of this household.");
        }

        if (target.Role == AppRole.HouseholdAdmin
            && newRole != AppRole.HouseholdAdmin
            && _members.Count(m => m.Role == AppRole.HouseholdAdmin) == 1)
        {
            return Result.Failure("Cannot demote the last Admin.");
        }

        target.ChangeRole(newRole);
        return Result.Success();
    }

    public void RegenerateInviteCode() => InviteCode = InviteCode.Generate();
}
