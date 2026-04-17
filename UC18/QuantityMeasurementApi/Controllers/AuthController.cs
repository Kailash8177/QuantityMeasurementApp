using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementbusinessLayer.Interfaces;

namespace QuantityMeasurementApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger      = logger;
        }

        /// <summary>Register a new user account</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            AuthResponseDTO result = await _authService.RegisterAsync(request, ip);

            if (!result.Success) return BadRequest(result);

            if (result.RefreshToken != null && result.ExpiresAt.HasValue)
                SetRefreshCookie(result.RefreshToken, result.ExpiresAt.Value.AddDays(7));

            return Ok(result);
        }

        /// <summary>Login with username and password</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            AuthResponseDTO result = await _authService.LoginAsync(request, ip);

            if (!result.Success) return Unauthorized(result);

            if (result.RefreshToken != null && result.ExpiresAt.HasValue)
                SetRefreshCookie(result.RefreshToken, result.ExpiresAt.Value.AddDays(7));

            return Ok(result);
        }

        /// <summary>Refresh access token using refresh token cookie or body</summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] LogoutRequestDTO? body = null)
        {
            string? token = Request.Cookies["refreshToken"] ?? body?.RefreshToken;
            if (string.IsNullOrEmpty(token))
                return BadRequest(new AuthResponseDTO { Success = false, Message = "Refresh token is required." });

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            AuthResponseDTO result = await _authService.RefreshTokenAsync(token, ip);

            if (!result.Success) return Unauthorized(result);

            if (result.RefreshToken != null && result.ExpiresAt.HasValue)
                SetRefreshCookie(result.RefreshToken, result.ExpiresAt.Value.AddDays(7));

            return Ok(result);
        }

        /// <summary>Logout and revoke all tokens</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO? body = null)
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new AuthResponseDTO { Success = false, Message = "User not authenticated." });

            long userId = long.Parse(userIdClaim);
            string? token = body?.RefreshToken ?? Request.Cookies["refreshToken"];
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

            AuthResponseDTO result = await _authService.LogoutAsync(token, userId, ip);
            Response.Cookies.Delete("refreshToken");
            return Ok(result);
        }

        /// <summary>Get current user profile</summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { Message = "User not authenticated." });

            var profile = await _authService.GetUserProfileAsync(long.Parse(userIdClaim));
            if (profile == null) return NotFound(new { Message = "User not found." });

            return Ok(profile);
        }

        /// <summary>Check authentication status (public)</summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult GetAuthStatus()
        {
            bool isAuth = User.Identity?.IsAuthenticated ?? false;
            return Ok(new
            {
                IsAuthenticated = isAuth,
                Username        = User.Identity?.Name,
                UserId          = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Message         = isAuth ? "User is logged in." : "User is not logged in."
            });
        }

        // ── Helper ──────────────────────────────────────────────────
        private void SetRefreshCookie(string refreshToken, DateTime expires)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure   = false,
                SameSite = SameSiteMode.Lax,
                Expires  = expires,
                Path     = "/"
            });
        }
    }
}
