namespace MarketplaceBackend.DTOs;

public record CreateReviewDto(int productId, int rating, string comment);
public record ReviewDto(int id, int rating, string comment, System.DateTime createdAt);
public record ReviewSummaryDto(int totalReviews, double averageRating);
