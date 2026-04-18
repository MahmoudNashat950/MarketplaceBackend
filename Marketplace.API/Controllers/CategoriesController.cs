using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public CategoriesController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromServices] MarketplaceBackend.Interfaces.ICategoryService svc)
    {
        var cats = await svc.GetAllAsync();
        return Ok(cats);
    }

    [HttpPost]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> Create([FromBody] MarketplaceBackend.DTOs.CreateCategoryDto dto, [FromServices] MarketplaceBackend.Interfaces.ICategoryService svc)
    {
        try
        {
            var cat = await svc.CreateAsync(dto);
            return Ok(new { id = cat.id, name = cat.name, message = "Category created successfully." });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
