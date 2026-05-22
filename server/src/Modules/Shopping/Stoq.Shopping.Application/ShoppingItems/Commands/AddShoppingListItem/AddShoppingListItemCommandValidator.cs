using FluentValidation;

namespace Stoq.Shopping.Application.ShoppingItems.Commands.AddShoppingListItem;

internal sealed class AddShoppingListItemCommandValidator : AbstractValidator<AddShoppingListItemCommand>
{
    public AddShoppingListItemCommandValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
