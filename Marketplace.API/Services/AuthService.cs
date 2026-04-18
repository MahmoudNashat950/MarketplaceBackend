using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MarketplaceBackend.Services;

public class AuthService : IAuthService
{
    private readonly MarketplaceDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(MarketplaceDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        if (dto is null) throw new ApplicationException("Invalid request.");

        if (string.IsNullOrWhiteSpace(dto.email))
            throw new ApplicationException("Email is required.");

        var normalizedEmail = dto.email.Trim().ToLowerInvariant();

        // Check duplicate email
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            throw new ApplicationException("Email already in use.");

        if (string.IsNullOrWhiteSpace(dto.name))
            throw new ApplicationException("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.password) || dto.password.Length < 6)
            throw new ApplicationException("Password is required and must be at least 6 characters.");

        var role = string.IsNullOrWhiteSpace(dto.role) ? "buyer" : dto.role;
        var roleLower = role.ToLowerInvariant();
        int roleValue = roleLower == "seller" ? 1 : 0;

        var user = new User
        {
            Name = dto.name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
            Role = roleValue
        };

        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log and rethrow a friendly message
            throw new ApplicationException("Unable to save user to the database.", ex);
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.email);
        if (user == null || !VerifyPassword(dto.password, user.PasswordHash))
            throw new ApplicationException("Invalid credentials.");

        var token = GenerateToken(user);
        var userDto = new UserDto(user.Id, user.Name, user.Email, user.Role == 1 ? "seller" : "buyer");
        return new AuthResponseDto(token, userDto);
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "VerySecretKey12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role == 1 ? "seller" : "buyer"),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
