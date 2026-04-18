using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceBackend.Services;

public class ReviewService : IReviewService
{
    private readonly MarketplaceDbContext _db;
    public ReviewService(MarketplaceDbContext db) { _db = db; }

    public async Task<ReviewDto> AddReviewAsync(CreateReviewDto dto, int userId)
    {
        if (dto.rating < 1 || dto.rating > 5) throw new ApplicationException("Rating must be between 1 and 5.");
        if (await _db.Ratings.AnyAsync(r => r.ProductId == dto.productId && r.UserId == userId)) 
            throw new ApplicationException("You have already reviewed this product.");
        
        var rating = new Rating { ProductId = dto.productId, UserId = userId, Value = dto.rating };
        _db.Ratings.Add(rating);
        await _db.SaveChangesAsync();
        return new ReviewDto(rating.Id, rating.Value, dto.comment ?? "", rating.CreatedAt);
    }

    public async Task<System.Collections.Generic.List<ReviewDto>> GetReviewsByProductAsync(int productId)
    {
        var reviews = await _db.Ratings.Where(r => r.ProductId == productId).ToListAsync();
        return reviews.Select(r => new ReviewDto(r.Id, r.Value, "", r.CreatedAt)).ToList();
    }

    public async Task<ReviewSummaryDto> GetSummaryAsync(int productId)
    {
        var total = await _db.Ratings.CountAsync(r => r.ProductId == productId);
        var avg = await _db.Ratings.Where(r => r.ProductId == productId).AverageAsync(r => (double?)r.Value) ?? 0.0;
        return new ReviewSummaryDto(total, Math.Round(avg, 2));
    }
}
