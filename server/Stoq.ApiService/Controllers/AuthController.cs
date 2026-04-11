using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stoq.Identity.Application.Auth.Commands.ForgotPassword;
using Stoq.Identity.Application.Auth.Commands.LoginUser;
using Stoq.Identity.Application.Auth.Commands.Logout;
using Stoq.Identity.Application.Auth.Commands.RefreshToken;
using Stoq.Identity.Application.Auth.Commands.RegisterUser;
using Stoq.Identity.Application.Auth.Commands.ResetPassword;
using Stoq.Identity.Application.Auth.Commands.SocialAuth;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Registers a new user account.</summary>
    /// <response code="200">Registration successful; returns auth tokens and user profile.</response>
    /// <response code="400">Validation failure or email already taken.</response>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        Result<AuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Authenticates an existing user with email and password.</summary>
    /// <response code="200">Login successful; returns auth tokens and user profile.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken ct)
    {
        Result<AuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Unauthorized(Problem(result.Error));
    }

    /// <summary>Authenticates or registers a user via a social provider token.</summary>
    /// <response code="200">Auth successful; returns auth tokens and user profile.</response>
    /// <response code="400">Invalid or expired social token.</response>
    [HttpPost("social")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Social([FromBody] SocialAuthCommand command, CancellationToken ct)
    {
        Result<AuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Exchanges an expired access token and valid refresh token for a new pair.</summary>
    /// <response code="200">Refresh successful; returns new auth tokens and user profile.</response>
    /// <response code="401">Refresh token is invalid or expired.</response>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        Result<AuthResponse> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Unauthorized(Problem(result.Error));
    }

    /// <summary>Revokes the caller's refresh token and ends the session.</summary>
    /// <response code="204">Logout successful.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);

        return NoContent();
    }

    /// <summary>
    /// Initiates the password reset flow by sending a 6-digit OTP to the user's email.
    /// Always returns 200 to prevent user enumeration.
    /// </summary>
    /// <response code="200">OTP sent (or silently suppressed if email is unknown).</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);

        return Ok();
    }

    /// <summary>Validates the 6-digit OTP and sets a new password.</summary>
    /// <response code="204">Password reset successful.</response>
    /// <response code="400">Invalid or expired OTP.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        Result result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(Problem(result.Error));
    }

    private static ProblemDetails Problem(string detail) =>
        new() { Detail = detail, Status = StatusCodes.Status400BadRequest };
}
