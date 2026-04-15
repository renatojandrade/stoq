using FluentValidation;

namespace Stoq.Inventory.Application.InventoryItems.Commands.ConsumeInventoryItem;

internal sealed class ConsumeInventoryItemCommandValidator : AbstractValidator<ConsumeInventoryItemCommand>
{
    public ConsumeInventoryItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
