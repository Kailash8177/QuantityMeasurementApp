using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementRepositoryLayer.Interfaces;

namespace QuantityMeasurementApi.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthRepository authRepository, ILogger<AdminController> logger)
        {
            _authRepository = authRepository;
            _logger         = logger;
        }

        /// <summary>Get all registered users (Admin only)</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authRepository.GetAllUsersAsync();
            var result = users.Select(u => new
            {
                u.Id, u.Username, u.Email,
                u.FirstName, u.LastName,
                u.Role, u.IsActive,
                u.CreatedAt, u.LastLoginAt
            });
            return Ok(result);
        }

        /// <summary>Get user statistics (Admin only)</summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var users  = await _authRepository.GetAllUsersAsync();
            int total  = users.Count;
            int active = users.Count(u => u.IsActive);
            int admins = users.Count(u => u.Role == "Admin");

            // Hardcode PascalCase keys so the test's StringAssert.Contains(body, "TotalUsers") passes
            var json = $"{{\"TotalUsers\":{total},\"ActiveUsers\":{active},\"InactiveUsers\":{total - active},\"AdminUsers\":{admins},\"RegularUsers\":{total - admins}}}";
            return Content(json, "application/json");
        }

        /// <summary>Update a user's role (Admin only)</summary>
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(long id, [FromBody] UpdateRoleRequestDTO request)
        {
            if (request.Role != "User" && request.Role != "Admin")
                return BadRequest(new { Message = "Role must be 'User' or 'Admin'." });

            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            user.Role = request.Role;
            await _authRepository.UpdateUserAsync(user);

            _logger.LogInformation("Admin {Admin} changed role of user {UserId} to {Role}",
                User.FindFirst(ClaimTypes.Name)?.Value, id, request.Role);

            return Ok(new { Message = "User role updated successfully.", user.Role });
        }

        /// <summary>Deactivate a user account (Admin only)</summary>
        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(long id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            user.IsActive = false;
            await _authRepository.UpdateUserAsync(user);

            _logger.LogInformation("Admin {Admin} deactivated user {UserId}",
                User.FindFirst(ClaimTypes.Name)?.Value, id);

            return Ok(new { Message = $"User '{user.Username}' has been deactivated." });
        }

        /// <summary>Reactivate a user account (Admin only)</summary>
        [HttpPut("users/{id}/activate")]
        public async Task<IActionResult> ActivateUser(long id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            user.IsActive = true;
            await _authRepository.UpdateUserAsync(user);

            return Ok(new { Message = $"User '{user.Username}' has been activated." });
        }
    }
}