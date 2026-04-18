using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface ICommentService
{
    Task<CommentDto> AddOrderCommentAsync(int orderId, CreateCommentDto dto, int userId);
    Task<System.Collections.Generic.List<CommentDto>> GetOrderCommentsAsync(int orderId);
}
