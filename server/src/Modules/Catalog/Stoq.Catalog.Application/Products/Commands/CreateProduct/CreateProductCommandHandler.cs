using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Aggregates;
using Stoq.Catalog.Domain.Enums;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Unit, ignoreCase: true, out ProductUnit unit))
        {
            return Result<ProductDto>.Failure($"Invalid product unit '{request.Unit}'.");
        }

        var product = Product.Create(
            request.HouseholdId,
            request.Name,
            request.Brand,
            request.Category,
            unit,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        await repository.AddAsync(product, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ProductDto>.Success(ToDto(product));
    }

    internal static ProductDto ToDto(Product p) =>
        new(p.Id, p.HouseholdId, p.Name, p.Brand, p.Category, p.Unit.ToString(), p.MinQuantity, p.ImageUrl, p.Barcode, p.Country, IsReference: p.HouseholdId is null);
}
