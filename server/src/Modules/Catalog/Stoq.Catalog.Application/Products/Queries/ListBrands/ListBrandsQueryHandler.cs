using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListBrands;

internal sealed class ListBrandsQueryHandler(IBrandRepository repository)
    : IRequestHandler<ListBrandsQuery, Result<IReadOnlyList<BrandDto>>>
{
    public async Task<Result<IReadOnlyList<BrandDto>>> Handle(ListBrandsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Brand> brands = await repository
            .SearchAsync(request.Name, request.HouseholdId, cancellationToken);

        IReadOnlyList<BrandDto> dtos = brands
            .Select(b => new BrandDto(b.Id, b.Name))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<BrandDto>>.Success(dtos);
    }
}
