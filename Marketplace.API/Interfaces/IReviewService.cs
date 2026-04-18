using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> AddReviewAsync(CreateReviewDto dto, int userId);
    Task<System.Collections.Generic.List<ReviewDto>> GetReviewsByProductAsync(int productId);
    Task<ReviewSummaryDto> GetSummaryAsync(int productId);
}
