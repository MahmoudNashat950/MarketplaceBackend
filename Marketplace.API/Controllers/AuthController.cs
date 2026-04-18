using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MarketplaceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MarketplaceDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(MarketplaceDbContext db, IConfiguration config, ILogger<AuthController> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] MarketplaceBackend.DTOs.RegisterDto dto, [FromServices] MarketplaceBackend.Interfaces.IAuthService auth)
    {
        if (dto is null)
            return BadRequest(new { message = "Request body is required." });

        // Basic validation - service will perform more checks
        if (string.IsNullOrWhiteSpace(dto.email) || string.IsNullOrWhiteSpace(dto.password) || string.IsNullOrWhiteSpace(dto.name))
            return BadRequest(new { message = "Name, email and password are required." });

        try
        {
            await auth.RegisterAsync(dto);
            return Ok(new { message = "Registration successful." });
        }
        catch (ApplicationException ex)
        {
            // Known application-level errors (validation, duplicate email, etc.)
            _logger.LogWarning(ex, "Registration failed due to application error");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Unexpected errors should not crash the process
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] MarketplaceBackend.DTOs.LoginDto dto, [FromServices] MarketplaceBackend.Interfaces.IAuthService auth)
    {
        try
        {
            var res = await auth.LoginAsync(dto);
            return Ok(new { token = res.token, user = new { id = res.user.id, name = res.user.name, email = res.user.email, role = res.user.role } });
        }
        catch (ApplicationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "VerySecretKey12345";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role == 1 ? "seller" : "buyer"),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

// DTO types are defined in `MarketplaceBackend.DTOs.AuthDtos` - remove local duplicates
