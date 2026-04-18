namespace MarketplaceBackend.DTOs;

public record CreateCommentDto(string text);
public record CommentDto(int id, string text, System.DateTime createdAt);
