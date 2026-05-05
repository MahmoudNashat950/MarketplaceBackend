using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.API.Controllers;

/// <summary>
/// Reviews controller
/// Handles product reviews and ratings
/// </summary>
[ApiController]
[Route("api/review")]
[Produces("application/json")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Add a review for a product (Buyer only)
    /// One review per user per product
    /// </summary>
    /// <param name="request">Review details</param>
    /// <returns>Created review</returns>
    /// <response code="200">Review added successfully</response>
    /// <response code="400">Invalid rating or duplicate review</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a buyer)</response>
    /// <response code="404">Product not found</response>
    [HttpPost]
    [Authorize(Roles = "buyer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var userId = GetCurrentUserId();
            var review = await _reviewService.AddReviewAsync(request, userId);
            return Ok(new
            {
                id = review.Id,
                productId = review.ProductId,
                rating = review.Rating,
                comment = review.Comment,
                createdAt = review.CreatedAt
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to add review: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding review");
            return StatusCode(500, new { message = "An error occurred while adding the review." });
        }
    }

    /// <summary>
    /// Get all reviews for a product (public endpoint)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of reviews</returns>
    /// <response code="200">Reviews retrieved successfully</response>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        try
        {
            var reviews = await _reviewService.GetReviewsByProductAsync(productId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
            return StatusCode(500, new { message = "An error occurred while retrieving reviews." });
        }
    }

    /// <summary>
    /// Get review summary for a product (public endpoint)
    /// Returns total review count and average rating
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Review summary</returns>
    /// <response code="200">Summary retrieved successfully</response>
    [HttpGet("summary/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewSummary(int productId)
    {
        try
        {
            var summary = await _reviewService.GetSummaryAsync(productId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving review summary for product {ProductId}", productId);
            return StatusCode(500, new { message = "An error occurred while retrieving the review summary." });
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
