namespace MarketplaceBackend.DTOs;

public record ProductDto(int id, string name, decimal price, int stock, int deliveryTimeInDays, string category, int categoryId, string? imageUrl, decimal? discount, double rating, int reviewsCount);

public record CreateProductDto(string Name, decimal Price, int Stock, int CategoryId, int DeliveryTimeInDays, decimal? Discount, string? ImageUrl);

public record UpdateProductDto(string Name, decimal Price, int Stock, int CategoryId, int DeliveryTimeInDays, decimal? Discount, string? ImageUrl);
