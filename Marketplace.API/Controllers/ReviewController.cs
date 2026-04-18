using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public ReviewController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Create([FromBody] MarketplaceBackend.DTOs.CreateReviewDto dto, [FromServices] MarketplaceBackend.Interfaces.IReviewService svc)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await svc.AddReviewAsync(dto, userId);
            return Ok(new { id = res.id, productId = dto.productId, rating = res.rating, comment = res.comment, createdAt = res.createdAt });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductReviews(int productId, [FromServices] MarketplaceBackend.Interfaces.IReviewService svc)
    {
        var reviews = await svc.GetReviewsByProductAsync(productId);
        return Ok(reviews);
    }

    [HttpGet("summary/{productId}")]
    public async Task<IActionResult> Summary(int productId, [FromServices] MarketplaceBackend.Interfaces.IReviewService svc)
    {
        var sum = await svc.GetSummaryAsync(productId);
        return Ok(sum);
    }
}

public record CreateReviewDto(int ProductId, int Rating, string Comment);
