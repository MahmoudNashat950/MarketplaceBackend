using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderDto dto, int buyerId);
    Task<System.Collections.Generic.List<OrderDto>> GetBuyerOrdersAsync(int buyerId);
    Task<System.Collections.Generic.List<OrderDto>> GetSellerOrdersAsync(int sellerId);
    Task UpdateStatusAsync(int id, string status, int sellerId);
}
