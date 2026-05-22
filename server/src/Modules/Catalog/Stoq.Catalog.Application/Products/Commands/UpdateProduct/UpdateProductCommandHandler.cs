using MediatR;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(IProductRepository repository, IBrandRepository brandRepository)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null || product.HouseholdId != request.RequestingHouseholdId)
        {
            return Result.NotFound("Product not found.");
        }

        Guid? brandId = await ResolveBrandIdAsync(request.BrandName, request.RequestingHouseholdId, cancellationToken);

        Result result = product.Update(
            request.Name,
            request.CategoryId,
            brandId,
            request.UnitId,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Guid?> ResolveBrandIdAsync(string? brandName, Guid householdId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            return null;
        }

        Brand? existing = await brandRepository
            .FindByNameAsync(brandName, householdId, ct);

        if (existing is not null)
        {
            return existing.Id;
        }

        var newBrand = Brand.Create(Guid.NewGuid(), brandName, householdId);
        await brandRepository.AddAsync(newBrand, ct);

        return newBrand.Id;
    }
}
