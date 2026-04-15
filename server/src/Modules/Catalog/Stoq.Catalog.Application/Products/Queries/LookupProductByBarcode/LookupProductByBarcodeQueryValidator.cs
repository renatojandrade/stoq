using FluentValidation;

namespace Stoq.Catalog.Application.Products.Queries.LookupProductByBarcode;

internal sealed class LookupProductByBarcodeQueryValidator : AbstractValidator<LookupProductByBarcodeQuery>
{
    public LookupProductByBarcodeQueryValidator()
    {
        RuleFor(q => q.Barcode)
            .NotEmpty()
            .Matches(@"^\d{8,14}$")
            .WithMessage("Barcode must be 8–14 digits.");
    }
}
