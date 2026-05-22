using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListUnits;

public sealed record ListUnitsQuery : IRequest<Result<IReadOnlyList<UnitDto>>>;
