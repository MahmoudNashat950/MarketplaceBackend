using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceBackend.Services;

public class CommentService : ICommentService
{
    private readonly MarketplaceDbContext _db;
    public CommentService(MarketplaceDbContext db) { _db = db; }

    public async Task<CommentDto> AddOrderCommentAsync(int orderId, CreateCommentDto dto, int userId)
    {
        var oc = new OrderComment { OrderId = orderId, UserId = userId, Text = dto.text };
        _db.OrderComments.Add(oc);
        await _db.SaveChangesAsync();
        return new CommentDto(oc.Id, oc.Text, oc.CreatedAt);
    }

    public async Task<System.Collections.Generic.List<CommentDto>> GetOrderCommentsAsync(int orderId)
    {
        var comments = await _db.OrderComments.Where(c => c.OrderId == orderId).ToListAsync();
        return comments.Select(c => new CommentDto(c.Id, c.Text, c.CreatedAt)).ToList();
    }
}
