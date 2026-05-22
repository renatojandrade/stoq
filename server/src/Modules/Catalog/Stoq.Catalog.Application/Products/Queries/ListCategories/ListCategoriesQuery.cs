using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.ListCategories;

public sealed record ListCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;
