using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.ApiService.DTOs;
using Stoq.Identity.Application.Notifications.Commands.RegisterDevicePushToken;
using Stoq.Identity.Application.Notifications.Commands.RemoveDevicePushToken;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    IMediator mediator,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>Registers or refreshes a device push token for the current user.</summary>
    /// <response code="204">Token registered.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterPushTokenRequest request, CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        await mediator.Send(new RegisterDevicePushTokenCommand(userId.Value, request.Token), ct);
        return NoContent();
    }

    /// <summary>Removes all push tokens for the current user (call on logout).</summary>
    /// <response code="204">Tokens removed.</response>
    [HttpDelete("token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveToken(CancellationToken ct)
    {
        Guid? userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        await mediator.Send(new RemoveDevicePushTokenCommand(userId.Value), ct);
        return NoContent();
    }
}
