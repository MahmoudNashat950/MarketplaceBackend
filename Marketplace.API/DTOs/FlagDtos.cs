namespace MarketplaceBackend.DTOs;

public record FlagSellerDto(int sellerId, string reason);
public record FlagBuyerDto(int buyerId, string reason);
