using Stoq.Shared.Contracts.Entities;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Common;
using Stoq.Shopping.Domain.Enums;

namespace Stoq.Shopping.Domain.Aggregates;

public sealed class ShoppingListItem : AggregateRoot<Guid>, IAuditableEntity
{
    // EF Core
    private ShoppingListItem()
    {
    }

    private ShoppingListItem(
        Guid id,
        Guid householdId,
        string productName,
        decimal quantity,
        string unit,
        string? notes,
        ShoppingItemSource source)
        : base(id)
    {
        HouseholdId = householdId;
        ProductName = productName;
        Quantity = quantity;
        Unit = unit;
        Notes = notes;
        Source = source;
        IsChecked = false;
    }

    public Guid HouseholdId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public bool IsChecked { get; private set; }

    public ShoppingItemSource Source { get; private set; }

    // Audit columns (IAuditableEntity)
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid UpdatedBy { get; set; }

    public static ShoppingListItem Create(
        Guid householdId,
        string productName,
        decimal quantity,
        string unit,
        string? notes,
        ShoppingItemSource source)
    {
        Guard.AgainstEmptyGuid(householdId, nameof(householdId));
        Guard.AgainstNullOrWhiteSpace(productName, nameof(productName));
        Guard.AgainstNullOrWhiteSpace(unit, nameof(unit));

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new ShoppingListItem(Guid.NewGuid(), householdId, productName, quantity, unit, notes, source);
    }

    public void CheckOff()
    {
        IsChecked = true;
    }

    public void Uncheck()
    {
        IsChecked = false;
    }
}
