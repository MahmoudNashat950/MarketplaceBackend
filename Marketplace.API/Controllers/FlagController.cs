using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlagController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public FlagController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpPost("seller")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> FlagSeller([FromBody] DTOs.FlagSellerDto dto, [FromServices] Interfaces.IFlagService svc)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await svc.FlagSellerAsync(dto, userId);
        return Ok(new { message = "Seller flagged successfully." });
    }

    [HttpPost("buyer")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> FlagBuyer([FromBody] DTOs.FlagBuyerDto dto, [FromServices] Interfaces.IFlagService svc)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await svc.FlagBuyerAsync(dto, userId);
        return Ok(new { message = "Buyer flagged successfully." });
    }
}

public record FlagSellerDto(int SellerId, string Reason);
public record FlagBuyerDto(int BuyerId, string Reason);
