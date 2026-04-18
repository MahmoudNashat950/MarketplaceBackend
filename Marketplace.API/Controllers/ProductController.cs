using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public ProductController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        var products = await svc.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        var products = await svc.SearchAsync(query);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        var p = await svc.GetByIdAsync(id);
        if (p == null) return NotFound(new { message = "Product not found." });
        return Ok(p);
    }

    [HttpPost]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> Create([FromBody] MarketplaceBackend.DTOs.CreateProductDto dto, [FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        var sellerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var created = await svc.CreateAsync(dto, sellerId);
            return Ok(created);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> Update(int id, [FromBody] MarketplaceBackend.DTOs.UpdateProductDto dto, [FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        try
        {
            var sellerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var updated = await svc.UpdateAsync(id, dto, sellerId);
            return Ok(updated);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> Delete(int id, [FromServices] MarketplaceBackend.Interfaces.IProductService svc)
    {
        try
        {
            var sellerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            await svc.DeleteAsync(id, sellerId);
            return Ok(new { message = "Product deleted successfully." });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
