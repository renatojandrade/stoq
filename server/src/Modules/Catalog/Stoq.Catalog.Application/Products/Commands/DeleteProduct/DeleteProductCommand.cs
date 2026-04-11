using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(
    Guid ProductId,
    Guid RequestingHouseholdId) : IRequest<Result>;
