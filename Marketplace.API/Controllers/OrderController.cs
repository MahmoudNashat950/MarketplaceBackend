using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public OrderController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Create([FromBody] DTOs.CreateOrderDto dto, [FromServices] Interfaces.IOrderService svc)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await svc.CreateAsync(dto, userId);
            return Ok(res);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> MyOrders([FromServices] Interfaces.IOrderService svc)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var orders = await svc.GetBuyerOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("seller")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> SellerOrders([FromServices] Interfaces.IOrderService svc)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var orders = await svc.GetSellerOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto, [FromServices] Interfaces.IOrderService svc)
    {
        try
        {
            var sellerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            await svc.UpdateStatusAsync(id, dto.status, sellerId);
            return Ok(new { message = "Order status updated." });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/comments")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> AddComment(int id, [FromBody] DTOs.CreateCommentDto dto, [FromServices] Interfaces.ICommentService svc)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var res = await svc.AddOrderCommentAsync(id, dto, userId);
            return Ok(new { id = res.id, text = res.text, createdAt = res.createdAt });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(int id, [FromServices] Interfaces.ICommentService svc)
    {
        var comments = await svc.GetOrderCommentsAsync(id);
        return Ok(comments);
    }
}

public record UpdateStatusDto(string status);
