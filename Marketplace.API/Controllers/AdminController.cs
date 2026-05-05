using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

     
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("users/{id}/ban")]
        public async Task<IActionResult> BanUser(int id)
        {
            await _adminService.BanUserAsync(id);
            return Ok(new { message = "User banned successfully" });
        }

       
        [HttpPost("users/{id}/unban")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            await _adminService.UnbanUserAsync(id);
            return Ok(new { message = "User unbanned successfully" });
        }


        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _adminService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _adminService.GetReportsAsync();
            return Ok(reports);
        }
    }
}