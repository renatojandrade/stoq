using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListBrands;

public sealed record ListBrandsQuery(Guid HouseholdId, string? Name) : IRequest<Result<IReadOnlyList<BrandDto>>>;
