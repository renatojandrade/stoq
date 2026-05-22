using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stoq.ApiService.DTOs;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Application.Receipts.Commands.ParseNfe;
using Stoq.Inventory.Application.Receipts.Queries.GetCorrectionDetail;
using Stoq.Inventory.Application.Receipts.Queries.GetRecentCorrections;
using Stoq.Inventory.Infrastructure.BackgroundServices;
using Stoq.Shared.Kernel.Common;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Controllers;

[ApiController]
[Route("api/receipts")]
[Authorize]
public sealed class ReceiptsController(
    IMediator mediator,
    ICurrentUserService currentUserService,
    IPhotoReceiptParserService photoParser,
    IOptions<NfeParseOptions> nfeOptions)
    : ControllerBase
{
    /// <summary>
    /// Parses a receipt photo using Claude Vision and returns the extracted product items.
    /// The caller must confirm and submit the items via POST /api/inventory/batch.
    /// </summary>
    /// <response code="200">Parsed result returned, including identified retailer name and a correctionId for tracking.</response>
    /// <response code="400">Image is missing or invalid.</response>
    /// <response code="502">Claude Vision API call failed.</response>
    [HttpPost("parse-photo")]
    [ProducesResponseType<ParseReceiptResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ParsePhoto([FromBody] ParseReceiptPhotoRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            return BadRequest(Problem("imageBase64 is required."));
        }

        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.ImageBase64);
        }
        catch (FormatException)
        {
            return BadRequest(Problem("imageBase64 is not valid base64."));
        }

        try
        {
            using var stream = new MemoryStream(imageBytes);
            ParseReceiptResultDto parsed = await photoParser.ParseAsync(stream, request.ContentType, householdId.Value, ct);
            return Ok(parsed);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                Problem($"Receipt parser unavailable: {ex.Message}", StatusCodes.Status502BadGateway));
        }
    }

    /// <summary>
    /// Enqueues an NF-e receipt parse job and returns immediately.
    /// The result is delivered via SignalR event <c>ReceiptCorrectionReady</c> on the receipts hub.
    /// </summary>
    /// <response code="202">Job accepted. Listen for ReceiptCorrectionReady / ReceiptCorrectionFailed on /hubs/receipts.</response>
    /// <response code="400">QR code URL is missing or user has no household.</response>
    /// <response code="429">Per-user pending job limit reached. Try again later.</response>
    [HttpPost("parse-nfe")]
    [ProducesResponseType<NfeJobAcceptedDto>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ParseNfe([FromBody] ParseNfeReceiptRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.QrCodeUrl))
        {
            return BadRequest(Problem("qrCodeUrl is required."));
        }

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

        var command = new ParseNfeCommand(
            request.QrCodeUrl,
            householdId.Value,
            userId.Value,
            nfeOptions.Value.MaxPendingJobsPerUser);

        Result<Guid> result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Accepted(new NfeJobAcceptedDto(result.Value));
        }

        if (result.IsTooManyRequests)
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                Problem(result.Error, StatusCodes.Status429TooManyRequests));
        }

        return BadRequest(Problem(result.Error));
    }

    /// <summary>Returns the caller's most recent NF-e parse corrections, newest first.</summary>
    /// <response code="200">List of corrections returned.</response>
    /// <response code="400">Caller has no household.</response>
    [HttpGet("corrections")]
    [ProducesResponseType<IReadOnlyList<ParseCorrectionSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCorrections([FromQuery] int limit = 10, CancellationToken ct = default)
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

        var query = new GetRecentCorrectionsQuery(householdId.Value, userId.Value, limit);
        IReadOnlyList<ParseCorrectionSummaryDto> result = await mediator.Send(query, ct);

        return Ok(result);
    }

    private static ProblemDetails Problem(string detail, int status = StatusCodes.Status400BadRequest) =>
        new() { Detail = detail, Status = status };

    /// <summary>Returns the parsed items for a specific correction, ready for user review.</summary>
    /// <response code="200">Correction detail returned.</response>
    /// <response code="403">Correction belongs to a different household.</response>
    /// <response code="404">Correction not found.</response>
    [HttpGet("corrections/{correctionId:guid}")]
    [ProducesResponseType<ParseCorrectionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCorrectionDetail(Guid correctionId, CancellationToken ct)
    {
        Guid? householdId = currentUserService.HouseholdId;
        if (householdId is null)
        {
            return BadRequest(Problem("User is not a member of any household."));
        }

        var query = new GetCorrectionDetailQuery(correctionId, householdId.Value);
        Result<ParseCorrectionDetailDto> result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.IsNotFound)
        {
            return NotFound(Problem(result.Error, StatusCodes.Status404NotFound));
        }

        if (result.IsForbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, Problem(result.Error, StatusCodes.Status403Forbidden));
        }

        return BadRequest(Problem(result.Error));
    }
}
