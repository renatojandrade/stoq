using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListCategories;

internal sealed class ListCategoriesQueryHandler(ICategoryRepository repository)
    : IRequestHandler<ListCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await repository
            .ListAllAsync(cancellationToken);

        IReadOnlyList<CategoryDto> dtos = categories
            .Select(c => new CategoryDto(c.Id, c.NameEn, c.NamePt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }
}
