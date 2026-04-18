namespace MarketplaceBackend.DTOs;

public record OrderItemDto(int productId, string productName, int quantity, decimal price);
public record OrderDto(int id, string status, System.DateTime? createdAt, decimal totalPrice, System.Collections.Generic.List<OrderItemDto> items);
public record CreateOrderDto(System.Collections.Generic.List<CreateOrderItemDto> items);
public record CreateOrderItemDto(int productId, int quantity);
