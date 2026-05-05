using Marketplace.Application.DTOs.OrderComments;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.API.Controllers;

/// <summary>
/// Orders controller
/// Handles order creation, management, and comments
/// </summary>
[ApiController]
[Route("api/order")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderCommentService _orderCommentService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        IOrderCommentService orderCommentService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _orderCommentService = orderCommentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order (Buyer only)
    /// </summary>
    /// <param name="request">Order details with items</param>
    /// <returns>Created order</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="400">Invalid request data or insufficient stock</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a buyer)</response>
    [HttpPost]
    [Authorize(Roles = "buyer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var buyerId = GetCurrentUserId();
            var order = await _orderService.CreateAsync(request, buyerId);
            return Ok(order);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to create order: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { message = "An error occurred while creating the order." });
        }
    }

    /// <summary>
    /// Get buyer's orders (Buyer only)
    /// </summary>
    /// <returns>List of buyer's orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a buyer)</response>
    [HttpGet("my")]
    [Authorize(Roles = "buyer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var buyerId = GetCurrentUserId();
            var orders = await _orderService.GetBuyerOrdersAsync(buyerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving buyer orders");
            return StatusCode(500, new { message = "An error occurred while retrieving orders." });
        }
    }

    /// <summary>
    /// Get seller's orders (Seller only)
    /// Returns orders containing products from this seller
    /// </summary>
    /// <returns>List of seller's orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    [HttpGet("seller")]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSellerOrders()
    {
        try
        {
            var sellerId = GetCurrentUserId();
            var orders = await _orderService.GetSellerOrdersAsync(sellerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seller orders");
            return StatusCode(500, new { message = "An error occurred while retrieving orders." });
        }
    }

    /// <summary>
    /// Update order status (Seller only, must own products in order)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">New status</param>
    /// <returns>Success message</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid status or ownership violation</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    /// <response code="404">Order not found</response>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { message = "Status is required." });

            var sellerId = GetCurrentUserId();
            await _orderService.UpdateStatusAsync(id, request.Status, sellerId);
            return Ok(new { message = "Order status updated successfully." });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to update order {OrderId} status: {Message}", id, ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", id);
            return StatusCode(500, new { message = "An error occurred while updating the order status." });
        }
    }

    /// <summary>
    /// Add a comment to an order (Buyer only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Comment text</param>
    /// <returns>Created comment</returns>
    /// <response code="200">Comment added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a buyer)</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id}/comments")]
    [Authorize(Roles = "buyer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateOrderCommentRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var userId = GetCurrentUserId();
            var comment = await _orderCommentService.AddCommentAsync(id, request, userId);
            return Ok(comment);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to add comment to order {OrderId}: {Message}", id, ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to order {OrderId}", id);
            return StatusCode(500, new { message = "An error occurred while adding the comment." });
        }
    }

    /// <summary>
    /// Get comments for an order (public endpoint)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>List of comments</returns>
    /// <response code="200">Comments retrieved successfully</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(int id)
    {
        try
        {
            var comments = await _orderCommentService.GetCommentsAsync(id);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for order {OrderId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving comments." });
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
