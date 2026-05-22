using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(IProductRepository repository, IBrandRepository brandRepository)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcodeExists = await repository
                .ExistsByBarcodeAsync(request.Barcode, cancellationToken);

            if (barcodeExists)
            {
                return Result<ProductDto>.Failure($"A product with barcode '{request.Barcode}' already exists in the catalog.");
            }
        }

        Guid? brandId = await ResolveBrandIdAsync(request.BrandName, request.HouseholdId, cancellationToken);

        var product = Product.Create(
            request.HouseholdId,
            request.Name,
            request.CategoryId,
            brandId,
            request.UnitId,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties for the DTO.
        Product? loaded = await repository.GetByIdAsync(product.Id, cancellationToken);

        return Result<ProductDto>.Success(ToDto(loaded!));
    }

    internal static ProductDto ToDto(Product p) =>
        new(
            p.Id,
            p.HouseholdId,
            p.Name,
            p.CategoryId,
            p.Category?.NameEn ?? string.Empty,
            p.Category?.NamePt ?? string.Empty,
            p.BrandId,
            p.Brand?.Name,
            p.UnitId,
            p.Unit?.Code ?? string.Empty,
            p.Unit?.NameEn ?? string.Empty,
            p.Unit?.NamePt ?? string.Empty,
            p.MinQuantity,
            p.ImageUrl,
            p.Barcode,
            p.Country,
            IsReference: p.HouseholdId is null);

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
