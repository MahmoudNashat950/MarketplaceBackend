using Marketplace.Application.DTOs.Products;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.API.Controllers;

/// <summary>
/// Products controller
/// Handles product CRUD operations and browsing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products (public endpoint)
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { message = "An error occurred while retrieving products." });
        }
    }

    /// <summary>
    /// Get product by ID (public endpoint)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the product." });
        }
    }

    /// <summary>
    /// Search products by name (public endpoint)
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>List of matching products</returns>
    /// <response code="200">Search completed successfully</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        try
        {
            var products = await _productService.SearchAsync(query);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", query);
            return StatusCode(500, new { message = "An error occurred while searching products." });
        }
    }

    /// <summary>
    /// Create a new product (Seller only)
    /// </summary>
    /// <param name="request">Product details</param>
    /// <returns>Created product</returns>
    /// <response code="200">Product created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    [HttpPost]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var sellerId = GetCurrentUserId();
            var product = await _productService.CreateAsync(request, sellerId);
            return Ok(product);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to create product: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "An error occurred while creating the product." });
        }
    }

    /// <summary>
    /// Update an existing product (Seller only, must own the product)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated product details</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid request data or ownership violation</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var sellerId = GetCurrentUserId();
            var product = await _productService.UpdateAsync(id, request, sellerId);
            return Ok(product);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to update product {ProductId}: {Message}", id, ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the product." });
        }
    }

    /// <summary>
    /// Delete a product (Seller only, must own the product)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Product deleted successfully</response>
    /// <response code="400">Ownership violation</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var sellerId = GetCurrentUserId();
            await _productService.DeleteAsync(id, sellerId);
            return Ok(new { message = "Product deleted successfully." });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete product {ProductId}: {Message}", id, ex.Message);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the product." });
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
