using Marketplace.Application.DTOs.Categories;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

/// <summary>
/// Categories controller
/// Handles product category management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories (public endpoint)
    /// </summary>
    /// <returns>List of all categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new { message = "An error occurred while retrieving categories." });
        }
    }

    /// <summary>
    /// Create a new category (Seller only)
    /// </summary>
    /// <param name="request">Category details</param>
    /// <returns>Created category</returns>
    /// <response code="200">Category created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized (not logged in)</response>
    /// <response code="403">Forbidden (not a seller)</response>
    [HttpPost]
    [Authorize(Roles = "seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var category = await _categoryService.CreateAsync(request);
            return Ok(new
            {
                id = category.Id,
                name = category.Name,
                message = "Category created successfully."
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Failed to create category: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "An error occurred while creating the category." });
        }
    }
}
