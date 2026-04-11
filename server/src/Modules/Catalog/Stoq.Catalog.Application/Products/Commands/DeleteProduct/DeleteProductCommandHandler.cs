using MediatR;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(IProductRepository repository)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(request.ProductId, cancellationToken).ConfigureAwait(false);

        if (product is null || product.HouseholdId != request.RequestingHouseholdId)
        {
            return Result.NotFound("Product not found.");
        }

        repository.Remove(product);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
