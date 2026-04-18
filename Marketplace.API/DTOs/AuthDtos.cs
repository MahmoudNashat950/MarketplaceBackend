namespace MarketplaceBackend.DTOs;

public record RegisterDto(string name, string email, string password, string role);
public record LoginDto(string email, string password);

public record UserDto(int id, string name, string email, string role);

public record AuthResponseDto(string token, UserDto user);
