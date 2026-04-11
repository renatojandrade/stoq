using Stoq.Catalog.Domain.Enums;
using Stoq.Catalog.Domain.Events;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Domain.Aggregates;

public sealed class Product : AggregateRoot<Guid>
{
    // EF Core
    private Product()
    {
    }

    private Product(
        Guid id,
        Guid householdId,
        string name,
        string? brand,
        string category,
        ProductUnit unit,
        decimal minQuantity,
        string? imageUrl,
        string? barcode)
        : base(id)
    {
        HouseholdId = householdId;
        Name = name;
        Brand = brand;
        Category = category;
        Unit = unit;
        MinQuantity = minQuantity;
        ImageUrl = imageUrl;
        Barcode = barcode;

        RaiseDomainEvent(new ProductCreatedDomainEvent(id, householdId));
    }

    public Guid HouseholdId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Brand { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public ProductUnit Unit { get; private set; }

    public decimal MinQuantity { get; private set; }

    public string? ImageUrl { get; private set; }

    public string? Barcode { get; private set; }

    // Audit columns
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public static Product Create(
        Guid householdId,
        string name,
        string? brand,
        string category,
        ProductUnit unit,
        decimal minQuantity,
        string? imageUrl,
        string? barcode)
    {
        Guard.AgainstEmptyGuid(householdId, nameof(householdId));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNullOrWhiteSpace(category, nameof(category));

        if (minQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minQuantity), "MinQuantity must be non-negative.");
        }

        return new Product(Guid.NewGuid(), householdId, name, brand, category, unit, minQuantity, imageUrl, barcode);
    }

    public Result Update(
        string name,
        string? brand,
        string category,
        ProductUnit unit,
        decimal minQuantity,
        string? imageUrl,
        string? barcode)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNullOrWhiteSpace(category, nameof(category));

        if (minQuantity < 0)
        {
            return Result.Failure("MinQuantity must be non-negative.");
        }

        Name = name;
        Brand = brand;
        Category = category;
        Unit = unit;
        MinQuantity = minQuantity;
        ImageUrl = imageUrl;
        Barcode = barcode;

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id, HouseholdId));

        return Result.Success();
    }
}
