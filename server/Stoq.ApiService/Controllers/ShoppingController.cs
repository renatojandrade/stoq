using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.ApiService.DTOs;
using Stoq.ApiService.Services;
using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Services;
using Stoq.Shopping.Application.Common.DTOs;
using Stoq.Shopping.Application.ShoppingItems.Commands.AddShoppingListItem;
using Stoq.Shopping.Application.ShoppingItems.Commands.CheckOffItem;
using Stoq.Shopping.Application.ShoppingItems.Commands.ClearCheckedItems;
using Stoq.Shopping.Application.ShoppingItems.Commands.RemoveShoppingListItem;
using Stoq.Shopping.Application.ShoppingItems.Commands.UncheckItem;
using Stoq.Shopping.Application.ShoppingItems.Queries.ListShoppingItems;
using Stoq.Shopping.Domain.Enums;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/shopping")]
[Authorize]
public sealed class ShoppingController(
    IMediator mediator,
    ICurrentUserService currentUserService,
    IHouseholdSyncService householdSyncService)
    : ControllerBase
{
    /// <summary>Lists all shopping list items for the caller's household.</summary>
    /// <response code="200">Shopping list returned.</response>
    /// <response code="400">Caller has no household.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ShoppingListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new ListShoppingItemsQuery(householdId.Value);
        Result<IReadOnlyList<ShoppingListItemDto>> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Adds an item to the household shopping list.</summary>
    /// <response code="201">Item added.</response>
    /// <response code="400">Validation failed or caller has no household.</response>
    [HttpPost]
    [ProducesResponseType<ShoppingListItemDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddShoppingListItemRequest request, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new AddShoppingListItemCommand(
            householdId.Value,
            request.ProductName,
            request.Quantity,
            request.Unit,
            request.Notes,
            ShoppingItemSource.Manual,
            userId.Value);

        Result<ShoppingListItemDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await householdSyncService.BroadcastShoppingListUpdatedAsync(householdId.Value, ct);
            return CreatedAtAction(nameof(List), result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Marks a shopping list item as checked off (purchased).</summary>
    /// <response code="200">Item checked off.</response>
    /// <response code="404">Item not found.</response>
    [HttpPost("{id:guid}/check")]
    [ProducesResponseType<ShoppingListItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckOff(Guid id, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new CheckOffItemCommand(id, householdId.Value);
        Result<ShoppingListItemDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await householdSyncService.BroadcastShoppingListUpdatedAsync(householdId.Value, ct);
            return Ok(result.Value);
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Unchecks a previously checked-off shopping list item.</summary>
    /// <response code="200">Item unchecked.</response>
    /// <response code="404">Item not found.</response>
    [HttpPost("{id:guid}/uncheck")]
    [ProducesResponseType<ShoppingListItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Uncheck(Guid id, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new UncheckItemCommand(id, householdId.Value);
        Result<ShoppingListItemDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await householdSyncService.BroadcastShoppingListUpdatedAsync(householdId.Value, ct);
            return Ok(result.Value);
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Removes a shopping list item.</summary>
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

        var command = new RemoveShoppingListItemCommand(id, householdId.Value);
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await householdSyncService.BroadcastShoppingListUpdatedAsync(householdId.Value, ct);
            return NoContent();
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Removes all checked-off items from the household shopping list.</summary>
    /// <response code="204">Checked items cleared.</response>
    [HttpDelete("checked")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearChecked(CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var command = new ClearCheckedItemsCommand(householdId.Value);
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await householdSyncService.BroadcastShoppingListUpdatedAsync(householdId.Value, ct);
            return NoContent();
        }

        return BadRequest(Problem(result.Error));
    }

    private static ProblemDetails Problem(string detail, int status = StatusCodes.Status400BadRequest) =>
        new() { Detail = detail, Status = status };
}
