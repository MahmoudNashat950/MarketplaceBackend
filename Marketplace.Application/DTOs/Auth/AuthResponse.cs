namespace Marketplace.Application.DTOs.Auth;

public record UserResponse(int Id, string Name, string Email, string Role);

public record AuthResponse(string Token, UserResponse User);
