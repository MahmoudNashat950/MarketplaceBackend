using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

/// <summary>
/// Authentication controller
/// Handles user registration and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Success message</returns>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Invalid request or email already in use</response>
    /// <response code="500">Server error</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name, email, and password are required." });

            await _authService.RegisterAsync(request);
            return Ok(new { message = "Registration successful." });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Registration failed with validation error: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid email or password</response>
    /// <response code="500">Server error</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
         try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required." });

                var response = await _authService.LoginAsync(request);

                return Ok(new
                {
                    token = response.Token,
                    user = new
                    {
                        id = response.User.Id,
                        name = response.User.Name,
                        email = response.User.Email,
                        role = response.User.Role // now FIXED (admin/seller/buyer)
                    }
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Server error." });
            }
        
    }
}
