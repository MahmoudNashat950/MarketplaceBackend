using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceBackend.Services;

public class CategoryService : ICategoryService
{
    private readonly MarketplaceDbContext _db;
    public CategoryService(MarketplaceDbContext db) { _db = db; }

    public async Task<System.Collections.Generic.List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories.Select(c => new CategoryDto(c.Id, c.Name)).ToListAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var cat = new Category { Name = dto.name };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return new CategoryDto(cat.Id, cat.Name);
    }
}
