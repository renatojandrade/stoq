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
        Guid? householdId,
        string name,
        string? brand,
        string category,
        ProductUnit unit,
        decimal minQuantity,
        string? imageUrl,
        string? barcode,
        string? country)
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
        Country = country;

        RaiseDomainEvent(new ProductCreatedDomainEvent(id, householdId ?? Guid.Empty));
    }

    /// <summary>The owning household. NULL means this is a global reference product.</summary>
    public Guid? HouseholdId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Brand { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public ProductUnit Unit { get; private set; }

    public decimal MinQuantity { get; private set; }

    public string? ImageUrl { get; private set; }

    public string? Barcode { get; private set; }

    /// <summary>Country this product is associated with (e.g. "BR", "US"). NULL means available to all countries.</summary>
    public string? Country { get; private set; }

    // Audit columns
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    /// <summary>Creates a household-owned product.</summary>
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

        return new Product(Guid.NewGuid(), householdId, name, brand, category, unit, minQuantity, imageUrl, barcode, country: null);
    }

    /// <summary>Creates a global reference product (not owned by any household).</summary>
    public static Product CreateReference(
        string name,
        string? brand,
        string category,
        ProductUnit unit,
        string? imageUrl,
        string? barcode,
        string? country)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNullOrWhiteSpace(category, nameof(category));

        return new Product(Guid.NewGuid(), householdId: null, name, brand, category, unit, minQuantity: 0, imageUrl, barcode, country);
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

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id, HouseholdId ?? Guid.Empty));

        return Result.Success();
    }
}
