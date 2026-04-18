using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task RegisterAsync(RegisterDto dto);
}
