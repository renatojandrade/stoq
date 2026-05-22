using Stoq.Shared.Contracts.Entities;

namespace Stoq.Catalog.Domain.Entities;

public sealed class Brand : IAuditableEntity
{
    // EF Core
    private Brand()
    {
    }

    private Brand(Guid id, string name, Guid? householdId)
    {
        Id = id;
        Name = name;
        HouseholdId = householdId;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>NULL means this is a global brand (visible to all households).</summary>
    public Guid? HouseholdId { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid UpdatedBy { get; set; }

    public static Brand Create(Guid id, string name, Guid? householdId = null) =>
        new(id, name, householdId);
}
