using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.ApiService.DTOs;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Products.Queries.GetProduct;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.InventoryItems.Commands.AddInventoryItem;
using Stoq.Inventory.Application.InventoryItems.Commands.ConsumeInventoryItem;
using Stoq.Inventory.Application.InventoryItems.Commands.RemoveInventoryItem;
using Stoq.Inventory.Application.InventoryItems.Queries.GetExpiringItems;
using Stoq.Inventory.Application.InventoryItems.Queries.ListInventoryItems;
using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public sealed class InventoryController(
    IMediator mediator,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>Lists all inventory items for the caller's household, with an optional name filter.</summary>
    /// <response code="200">Inventory list returned.</response>
    /// <response code="400">Caller is not a member of any household.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InventoryItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] string? name, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new ListInventoryItemsQuery(householdId.Value, name);
        Result<IReadOnlyList<InventoryItemDto>> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Returns inventory items expiring within the given number of days (default 7).</summary>
    /// <response code="200">Expiring items returned.</response>
    /// <response code="400">Caller has no household.</response>
    [HttpGet("expiring")]
    [ProducesResponseType<IReadOnlyList<InventoryItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExpiring([FromQuery] int daysAhead = 7, CancellationToken ct = default)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new GetExpiringItemsQuery(householdId.Value, daysAhead);
        Result<IReadOnlyList<InventoryItemDto>> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>
    /// Adds a product to inventory. Product details are resolved from the catalog and snapshotted.
    /// </summary>
    /// <response code="201">Item added to inventory.</response>
    /// <response code="404">Product not found in catalog.</response>
    /// <response code="400">Validation failed or caller has no household.</response>
    [HttpPost]
    [ProducesResponseType<InventoryItemDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddInventoryItemRequest request, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        // Resolve product snapshot from catalog (composition root is allowed to reference both modules).
        var productQuery = new GetProductQuery(request.ProductId, householdId.Value);
        Result<ProductDto> productResult = await mediator.Send(productQuery, ct);

        if (productResult.IsNotFound)
        {
            return NotFound(Problem($"Product '{request.ProductId}' not found.", StatusCodes.Status404NotFound));
        }

        if (!productResult.IsSuccess)
        {
            return BadRequest(Problem(productResult.Error));
        }

        ProductDto product = productResult.Value;
        var command = new AddInventoryItemCommand(
            householdId.Value,
            product.Id,
            product.Name,
            product.Brand,
            product.ImageUrl,
            request.Quantity,
            product.MinQuantity,
            request.Unit,
            request.ExpiryDate,
            request.Notes);

        Result<InventoryItemDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(List), result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Consumes (reduces) an inventory item's quantity.</summary>
    /// <response code="200">Updated inventory item returned.</response>
    /// <response code="404">Item not found.</response>
    /// <response code="400">Amount exceeds available quantity or other validation error.</response>
    [HttpPost("{id:guid}/consume")]
    [ProducesResponseType<InventoryItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Consume(Guid id, [FromBody] ConsumeInventoryItemRequest request, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new ConsumeInventoryItemCommand(id, householdId.Value, request.Amount);
        Result<InventoryItemDto> result = await mediator.Send(command, ct);

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

    /// <summary>Removes an inventory item.</summary>
    /// <response code="204">Item removed.</response>
    /// <response code="404">Item not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new RemoveInventoryItemCommand(id, householdId.Value);
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
