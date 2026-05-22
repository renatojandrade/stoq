using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.ApiService.DTOs;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Identity.Application.Households.Commands.ChangeMemberRole;
using Stoq.Identity.Application.Households.Commands.CreateHousehold;
using Stoq.Identity.Application.Households.Commands.JoinHousehold;
using Stoq.Identity.Application.Households.Commands.RegenerateInviteCode;
using Stoq.Identity.Application.Households.Commands.RemoveMember;
using Stoq.Identity.Application.Households.Queries.GetMyHousehold;
using Stoq.Identity.Domain.Enums;
using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/households")]
[Authorize]
public sealed class HouseholdsController(
    IMediator mediator,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>Creates a new household with the caller as Admin.</summary>
    /// <response code="201">Household created successfully.</response>
    /// <response code="400">Caller is already a member of another household, or validation failed.</response>
    [HttpPost]
    [ProducesResponseType<HouseholdAuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateHouseholdRequest request, CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<HouseholdCountry>(request.Country, ignoreCase: true, out HouseholdCountry country))
        {
            return BadRequest(Problem("Invalid country. Supported values: BR, US."));
        }

        var command = new CreateHouseholdCommand(request.Name, userId.Value, country);
        Result<HouseholdAuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Get), result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Joins an existing household using a 6-character invite code.</summary>
    /// <response code="200">Successfully joined the household.</response>
    /// <response code="400">Invalid or expired invite code, or caller is already a member.</response>
    [HttpPost("join")]
    [ProducesResponseType<HouseholdAuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Join([FromBody] JoinHouseholdRequest request, CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new JoinHouseholdCommand(request.InviteCode, userId.Value);
        Result<HouseholdAuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Returns the caller's household with full member list.</summary>
    /// <response code="200">Household details returned.</response>
    /// <response code="404">Caller is not a member of any household.</response>
    [HttpGet]
    [ProducesResponseType<HouseholdDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = new GetMyHouseholdQuery(userId.Value);
        Result<HouseholdDetailDto> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
    }

    /// <summary>Removes a member from the household. Admin only.</summary>
    /// <response code="204">Member removed.</response>
    /// <response code="400">Cannot remove last admin or member not found.</response>
    /// <response code="403">Caller is not an admin.</response>
    /// <response code="404">Household not found.</response>
    [HttpDelete("{householdId:guid}/members/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid householdId, Guid targetUserId, CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new RemoveMemberCommand(householdId, targetUserId, userId.Value);
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        if (result.IsForbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Problem(result.Error, StatusCodes.Status403Forbidden));
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Partially updates a member (currently: role). Admin only.</summary>
    /// <response code="204">Role updated.</response>
    /// <response code="400">Invalid role or last admin demotion.</response>
    /// <response code="403">Caller is not an admin.</response>
    /// <response code="404">Household not found.</response>
    [HttpPatch("{householdId:guid}/members/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchMember(
        Guid householdId,
        Guid targetUserId,
        [FromBody] ChangeMemberRoleRequest request,
        CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new ChangeMemberRoleCommand(householdId, targetUserId, request.Role, userId.Value);
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        if (result.IsForbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Problem(result.Error, StatusCodes.Status403Forbidden));
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Regenerates the household invite code. Admin only.</summary>
    /// <response code="200">New invite code returned.</response>
    /// <response code="403">Caller is not an admin.</response>
    /// <response code="404">Household not found.</response>
    [HttpPost("{householdId:guid}/invite-code/regenerate")]
    [ProducesResponseType<HouseholdDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateInviteCode(Guid householdId, CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new RegenerateInviteCodeCommand(householdId, userId.Value);
        Result<HouseholdDto> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.IsForbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Problem(result.Error, StatusCodes.Status403Forbidden));
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
