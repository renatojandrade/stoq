using MediatR;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Catalog.Application.Products.Queries.LookupProductByBarcode;

public sealed record LookupProductByBarcodeQuery(string Barcode) : IRequest<Result<ProductLookupDto>>;
