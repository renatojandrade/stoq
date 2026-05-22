using FluentValidation;

namespace Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;

internal sealed class AddInventoryItemCommandValidator : AbstractValidator<AddInventoryItemCommand>
{
    public AddInventoryItemCommandValidator()
    {
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductBrand).MaximumLength(200).When(x => x.ProductBrand is not null);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
        RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Expiry date cannot be in the past.")
            .When(x => x.ExpiryDate is not null);
    }
}
