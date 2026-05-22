using Stoq.Inventory.Domain.Events;
using Stoq.Shared.Contracts.Entities;
using Stoq.Shared.Kernel.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Domain.Aggregates;

public sealed class InventoryItem : AggregateRoot<Guid>, IAuditableEntity
{
    // EF Core
    private InventoryItem()
    {
    }

    private InventoryItem(
        Guid id,
        Guid householdId,
        Guid productId,
        string productName,
        string? productBrand,
        string? productImageUrl,
        decimal quantity,
        decimal minQuantity,
        string unit,
        DateOnly? expiryDate,
        string? notes)
        : base(id)
    {
        HouseholdId = householdId;
        ProductId = productId;
        ProductName = productName;
        ProductBrand = productBrand;
        ProductImageUrl = productImageUrl;
        Quantity = quantity;
        MinQuantity = minQuantity;
        Unit = unit;
        ExpiryDate = expiryDate;
        Notes = notes;

        RaiseDomainEvent(new InventoryItemAddedDomainEvent(id, householdId));
    }

    public Guid HouseholdId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public string? ProductBrand { get; private set; }

    public string? ProductImageUrl { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal MinQuantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public DateOnly? ExpiryDate { get; private set; }

    public string? Notes { get; private set; }

    public bool IsStockLow => Quantity <= MinQuantity && MinQuantity > 0;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid UpdatedBy { get; set; }

    public static InventoryItem Create(
        Guid householdId,
        Guid productId,
        string productName,
        string? productBrand,
        string? productImageUrl,
        decimal quantity,
        decimal minQuantity,
        string unit,
        DateOnly? expiryDate,
        string? notes)
    {
        Guard.AgainstEmptyGuid(householdId, nameof(householdId));
        Guard.AgainstEmptyGuid(productId, nameof(productId));
        Guard.AgainstNullOrWhiteSpace(productName, nameof(productName));
        Guard.AgainstNullOrWhiteSpace(unit, nameof(unit));

        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be non-negative.");
        }

        return new InventoryItem(
            Guid.NewGuid(),
            householdId,
            productId,
            productName,
            productBrand,
            productImageUrl,
            quantity,
            minQuantity,
            unit,
            expiryDate,
            notes);
    }

    public Result Consume(decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("Amount to consume must be greater than zero.");
        }

        if (amount > Quantity)
        {
            return Result.Failure($"Cannot consume {amount} {Unit} — only {Quantity} {Unit} in stock.");
        }

        Quantity -= amount;

        RaiseDomainEvent(new InventoryItemConsumedDomainEvent(
            Id,
            HouseholdId,
            ProductId,
            ProductName,
            Quantity,
            MinQuantity,
            Unit));

        return Result.Success();
    }

    public void AddQuantity(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        Quantity += amount;
    }

    public void UpdateDetails(string? notes, DateOnly? expiryDate)
    {
        Notes = notes;
        ExpiryDate = expiryDate;
    }
}
