using Marketplace.Application.DTOs.Flags;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.API.Controllers;

/// <summary>
/// Flags controller
/// Handles user flagging/reporting
/// </summary>
[ApiController]
[Route("api/flag")]
[Produces("application/json")]
public class FlagController : ControllerBase
{
    private readonly IFlagService _flagService;
    private readonly ILogger<FlagController> _logger;

    public FlagController(IFlagService flagService, ILogger<FlagController> logger)
    {
        _flagService = flagService;
        _logger = logger;
    }

    /// <summary>
    /// Flag/report a seller (Buyer only)
    /// </summary>
    /// <param name="request">Flag details with seller ID and reason</param>
    /// <returns>Success message</returns>
    /// <response code="200">Seller flagged successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a buyer)</response>
    /// <response code="404">Seller not found</response>
    [HttpPost("seller")]
    [Authorize(Roles = "buyer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagSeller([FromBody] FlagSellerRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var reporterId = GetCurrentUserId();
            await _flagService.FlagSellerAsync(request, reporterId);
            return Ok(new { message = "Seller flagged successfully." });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to flag seller: {Message}", ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging seller");
            return StatusCode(500, new { message = "An error occurred while flagging the seller." });
        }
    }

    /// <summary>
    /// Flag/report a buyer (Seller only)
    /// </summary>
    /// <param name="request">Flag details with buyer ID and reason</param>
    /// <returns>Success message</returns>
    /// <response code="200">Buyer flagged successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    /// <response code="404">Buyer not found</response>
    [HttpPost("buyer")]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagBuyer([FromBody] FlagBuyerRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var reporterId = GetCurrentUserId();
            await _flagService.FlagBuyerAsync(request, reporterId);
            return Ok(new { message = "Buyer flagged successfully." });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to flag buyer: {Message}", ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging buyer");
            return StatusCode(500, new { message = "An error occurred while flagging the buyer." });
        }
    }

    /// <summary>
    /// Extract current user ID from JWT claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            throw new ApplicationException("Invalid user ID in token.");

        return userId;
    }
}
