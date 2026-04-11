using MediatR;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(request.ProductId, cancellationToken).ConfigureAwait(false);

        if (product is null || product.HouseholdId != request.RequestingHouseholdId)
        {
            return Result.NotFound("Product not found.");
        }

        if (!Enum.TryParse(request.Unit, ignoreCase: true, out ProductUnit unit))
        {
            return Result.Failure($"Invalid product unit '{request.Unit}'.");
        }

        Result result = product.Update(
            request.Name,
            request.Brand,
            request.Category,
            unit,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
