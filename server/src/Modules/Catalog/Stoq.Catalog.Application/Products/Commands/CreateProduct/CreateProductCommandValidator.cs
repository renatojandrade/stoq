using FluentValidation;

namespace Stoq.Catalog.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandName).MaximumLength(200).When(x => x.BrandName is not null);
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).MaximumLength(2000).When(x => x.ImageUrl is not null);
        RuleFor(x => x.Barcode).MaximumLength(14).When(x => x.Barcode is not null);
    }
}
