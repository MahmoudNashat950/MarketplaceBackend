using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceBackend.Services;

public class ProductService : IProductService
{
    private readonly MarketplaceDbContext _db;
    public ProductService(MarketplaceDbContext db) { _db = db; }

    public async Task<System.Collections.Generic.List<ProductDto>> GetAllAsync()
    {
        var products = await _db.Products.Include(p => p.Category).Include(p => p.RatingProducts).ToListAsync();
        return products.Select(p => Map(p)).ToList();
    }

    public async Task<System.Collections.Generic.List<ProductDto>> SearchAsync(string query)
    {
        var products = await _db.Products.Include(p => p.Category).Where(p => p.Name.Contains(query)).ToListAsync();
        return products.Select(p => Map(p)).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _db.Products.Include(p => p.Category).Include(p => p.RatingProducts).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return null;
        return Map(p);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId)
    {
        var p = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            DeliveryTimeInDays = dto.DeliveryTimeInDays,
            Discount = dto.Discount,
            ImageUrl = dto.ImageUrl,
            SellerId = sellerId
        };
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return Map(p);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, int sellerId)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) throw new ApplicationException("Product not found.");
        if (p.SellerId != sellerId) throw new ApplicationException("You can only update your own products.");
        
        p.Name = dto.Name;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        p.CategoryId = dto.CategoryId;
        p.DeliveryTimeInDays = dto.DeliveryTimeInDays;
        p.Discount = dto.Discount;
        p.ImageUrl = dto.ImageUrl;
        await _db.SaveChangesAsync();
        return Map(p);
    }

    public async Task DeleteAsync(int id, int sellerId)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) throw new ApplicationException("Product not found.");
        if (p.SellerId != sellerId) throw new ApplicationException("You can only delete your own products.");
        
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
    }

    private ProductDto Map(Product p)
    {
        var ratings = p.RatingProducts ?? new System.Collections.Generic.List<Rating>();
        var avg = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Value);
        return new ProductDto(p.Id, p.Name, p.Price, p.Stock, p.DeliveryTimeInDays, p.Category?.Name ?? "", p.CategoryId, p.ImageUrl, p.Discount, Math.Round(avg,2), ratings.Count);
    }
}
