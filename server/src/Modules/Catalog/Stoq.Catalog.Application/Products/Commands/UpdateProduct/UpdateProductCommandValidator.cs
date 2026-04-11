using FluentValidation;
using Stoq.Catalog.Domain.Enums;

namespace Stoq.Catalog.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private static readonly string[] ValidUnits = Enum.GetNames<ProductUnit>();

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.RequestingHouseholdId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).MaximumLength(200).When(x => x.Brand is not null);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().Must(u => ValidUnits.Contains(u, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Unit must be one of: {string.Join(", ", ValidUnits)}.");
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).MaximumLength(2000).When(x => x.ImageUrl is not null);
        RuleFor(x => x.Barcode).MaximumLength(14).When(x => x.Barcode is not null);
    }
}
