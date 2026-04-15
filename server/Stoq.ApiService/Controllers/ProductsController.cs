using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.ApiService.DTOs;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Products.Commands.CreateProduct;
using Stoq.Catalog.Application.Products.Commands.DeleteProduct;
using Stoq.Catalog.Application.Products.Commands.UpdateProduct;
using Stoq.Catalog.Application.Products.Queries.GetProduct;
using Stoq.Catalog.Application.Products.Queries.ListProducts;
using Stoq.Catalog.Application.Products.Queries.LookupProductByBarcode;
using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(
    IMediator mediator,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>Looks up a product by barcode from Open Food Facts. Returns pre-filled data to seed the product form.</summary>
    /// <response code="200">Product data found.</response>
    /// <response code="404">No product found for this barcode.</response>
    /// <response code="400">Barcode is not 8–14 digits.</response>
    [HttpGet("barcode/{code}")]
    [ProducesResponseType<ProductLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LookupBarcode(string code, CancellationToken ct)
    {
        var query = new LookupProductByBarcodeQuery(code);
        Result<ProductLookupDto> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Lists products for the caller's household, with optional name/category filters.</summary>
    /// <response code="200">Product list returned.</response>
    /// <response code="400">Caller is not a member of any household.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] string? name,
        [FromQuery] string? category,
        CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new ListProductsQuery(householdId.Value, name, category);
        Result<IReadOnlyList<ProductDto>> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Returns a single product by ID.</summary>
    /// <response code="200">Product returned.</response>
    /// <response code="404">Product not found or does not belong to caller's household.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new GetProductQuery(id, householdId.Value);
        Result<ProductDto> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
    }

    /// <summary>Creates a new product in the caller's household.</summary>
    /// <response code="201">Product created.</response>
    /// <response code="400">Validation failed or caller has no household.</response>
    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new CreateProductCommand(
            householdId.Value,
            request.Name,
            request.Brand,
            request.Category,
            request.Unit,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        Result<ProductDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Updates an existing product.</summary>
    /// <response code="204">Product updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Product not found or does not belong to caller's household.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new UpdateProductCommand(
            id,
            householdId.Value,
            request.Name,
            request.Brand,
            request.Category,
            request.Unit,
            request.MinQuantity,
            request.ImageUrl,
            request.Barcode);

        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Deletes a product from the caller's household.</summary>
    /// <response code="204">Product deleted.</response>
    /// <response code="404">Product not found or does not belong to caller's household.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new DeleteProductCommand(id, householdId.Value);
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    private static ProblemDetails Problem(string detail, int status = StatusCodes.Status400BadRequest) =>
        new() { Detail = detail, Status = status };
}
