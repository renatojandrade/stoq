using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;
using Stoq.Catalog.Domain.Entities;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListUnits;

internal sealed class ListUnitsQueryHandler(IUnitRepository repository)
    : IRequestHandler<ListUnitsQuery, Result<IReadOnlyList<UnitDto>>>
{
    public async Task<Result<IReadOnlyList<UnitDto>>> Handle(ListUnitsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<UnitOfMeasure> units = await repository
            .ListAllAsync(cancellationToken);

        IReadOnlyList<UnitDto> dtos = units
            .Select(u => new UnitDto(u.Id, u.Code, u.NameEn, u.NamePt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<UnitDto>>.Success(dtos);
    }
}
