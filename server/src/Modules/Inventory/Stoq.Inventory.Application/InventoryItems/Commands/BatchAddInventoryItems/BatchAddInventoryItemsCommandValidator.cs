using FluentValidation;

namespace Stoq.Inventory.Application.InventoryItems.Commands.BatchAddInventoryItems;

internal sealed class BatchAddInventoryItemsCommandValidator : AbstractValidator<BatchAddInventoryItemsCommand>
{
    public BatchAddInventoryItemsCommandValidator()
    {
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.Unit).NotEmpty().MaximumLength(20);
            item.RuleFor(i => i.Notes).MaximumLength(500).When(i => i.Notes is not null);
        });
    }
}
