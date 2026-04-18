using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MarketplaceBackend.Enums;

namespace MarketplaceBackend.Services;

public class OrderService : IOrderService
{
    private readonly MarketplaceDbContext _db;
    public OrderService(MarketplaceDbContext db) { _db = db; }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, int buyerId)
    {
        var order = new Order { BuyerId = buyerId, Status = (int)OrderStatus.Pending };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        decimal total = 0;
        var items = new System.Collections.Generic.List<OrderItemDto>();
        foreach (var it in dto.items)
        {
            var product = await _db.Products.FindAsync(it.productId);
            if (product == null) throw new ApplicationException($"Product {it.productId} not found.");
            if (product.Stock < it.quantity) throw new ApplicationException($"Insufficient stock for product {product.Id}.");
            product.Stock -= it.quantity;
            var oi = new OrderItem { OrderId = order.Id, ProductId = product.Id, Quantity = it.quantity, Price = product.Price };
            _db.OrderItems.Add(oi);
            total += product.Price * it.quantity;
            items.Add(new OrderItemDto(oi.ProductId, product.Name, oi.Quantity, oi.Price));
        }

        await _db.SaveChangesAsync();
        return new OrderDto(order.Id, OrderStatus.Pending.ToString(), order.CreatedAt, total, items);
    }

    public async Task<System.Collections.Generic.List<OrderDto>> GetBuyerOrdersAsync(int buyerId)
    {
        var orders = await _db.Orders.Where(o => o.BuyerId == buyerId).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToListAsync();
        return orders.Select(o => new OrderDto(o.Id, ((OrderStatus)o.Status).ToString(), o.CreatedAt, o.OrderItems.Sum(i => i.Price * i.Quantity), o.OrderItems.Select(i => new OrderItemDto(i.ProductId, i.Product.Name, i.Quantity, i.Price)).ToList())).ToList();
    }

    public async Task<System.Collections.Generic.List<OrderDto>> GetSellerOrdersAsync(int sellerId)
    {
        var orderItems = await _db.OrderItems.Include(oi => oi.Product).Include(oi => oi.Order).Where(oi => oi.Product.SellerId == sellerId).ToListAsync();
        var grouped = orderItems.GroupBy(oi => oi.OrderId);
        var list = new System.Collections.Generic.List<OrderDto>();
        foreach (var g in grouped)
        {
            var order = g.First().Order;
            var items = g.Select(i => new OrderItemDto(i.ProductId, i.Product.Name, i.Quantity, i.Price)).ToList();
            var total = items.Sum(i => i.price * i.quantity);
            list.Add(new OrderDto(order.Id, ((OrderStatus)order.Status).ToString(), order.CreatedAt, total, items));
        }
        return list;
    }

    public async Task UpdateStatusAsync(int id, string status, int sellerId)
    {
        var order = await _db.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) throw new ApplicationException("Order not found.");
        
        var hasSellerProducts = order.OrderItems.Any(oi => oi.Product.SellerId == sellerId);
        if (!hasSellerProducts) throw new ApplicationException("You can only update orders containing your products.");
        
        if (!Enum.TryParse<OrderStatus>(status, out var st)) throw new ApplicationException("Invalid status.");
        order.Status = (int)st;
        await _db.SaveChangesAsync();
    }
}
