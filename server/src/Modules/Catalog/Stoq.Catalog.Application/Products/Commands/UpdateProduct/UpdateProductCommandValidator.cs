using FluentValidation;

namespace Stoq.Catalog.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.RequestingHouseholdId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandName).MaximumLength(200).When(x => x.BrandName is not null);
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).MaximumLength(2000).When(x => x.ImageUrl is not null);
        RuleFor(x => x.Barcode).MaximumLength(14).When(x => x.Barcode is not null);
    }
}
