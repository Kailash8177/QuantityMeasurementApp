using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementbusinessLayer.Interfaces;

namespace QuantityMeasurementbusinessLayer
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration  _config;

        public AuthServiceImpl(IAuthRepository authRepository, IConfiguration config)
        {
            _authRepository = authRepository;
            _config         = config;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, string ipAddress)
        {
            // Check username taken
            if (await _authRepository.GetUserByUsernameAsync(request.Username) != null)
                return Fail("Username is already taken.");

            // Check email taken
            if (await _authRepository.GetUserByEmailAsync(request.Email) != null)
                return Fail("Email is already registered.");

            var user = new UserEntity
            {
                Username     = request.Username,
                Email        = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName    = request.FirstName,
                LastName     = request.LastName,
                Role         = "User",
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };

            user = await _authRepository.CreateUserAsync(user);
            return await BuildTokenResponse(user, ipAddress, "Registration successful.");
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, string ipAddress)
        {
            var user = await _authRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Fail("Invalid username or password.");

            if (!user.IsActive)
                return Fail("Account is deactivated.");

            user.LastLoginAt = DateTime.UtcNow;
            await _authRepository.UpdateUserAsync(user);

            return await BuildTokenResponse(user, ipAddress, "Login successful.");
        }

        public async Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var existing = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (existing == null || !existing.IsActive)
                return Fail("Invalid or expired refresh token.");

            var user = await _authRepository.GetUserByIdAsync(existing.UserId);
            if (user == null || !user.IsActive)
                return Fail("User not found or deactivated.");

            await _authRepository.RevokeAllUserTokensAsync(user.Id, ipAddress);
            return await BuildTokenResponse(user, ipAddress, "Token refreshed.");
        }

        public async Task<AuthResponseDTO> LogoutAsync(string? refreshToken, long userId, string ipAddress)
        {
            await _authRepository.RevokeAllUserTokensAsync(userId, ipAddress);
            return new AuthResponseDTO { Success = true, Message = "Logged out successfully." };
        }

        public async Task<UserInfoDTO?> GetUserProfileAsync(long userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            return user == null ? null : ToUserInfo(user);
        }

        // ─── Helpers ───────────────────────────────────────────────
        private async Task<AuthResponseDTO> BuildTokenResponse(UserEntity user, string ipAddress, string message)
        {
            var (accessToken, expiresAt) = GenerateJwtToken(user);
            string refresh               = GenerateRefreshToken();

            await _authRepository.CreateRefreshTokenAsync(new RefreshTokenEntity
            {
                UserId      = user.Id,
                Token       = refresh,
                ExpiresAt   = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryInDays", 7)),
                CreatedAt   = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            return new AuthResponseDTO
            {
                Success      = true,
                Message      = message,
                AccessToken  = accessToken,
                RefreshToken = refresh,
                ExpiresAt    = expiresAt,
                User         = ToUserInfo(user)
            };
        }

        private (string token, DateTime expiresAt) GenerateJwtToken(UserEntity user)
        {
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt   = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:TokenExpiryInMinutes", 60));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email,      user.Email),
                new Claim(ClaimTypes.NameIdentifier,          user.Id.ToString()),
                new Claim(ClaimTypes.Name,                    user.Username),
                new Claim(ClaimTypes.Role,                    user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static AuthResponseDTO Fail(string message) =>
            new AuthResponseDTO { Success = false, Message = message };

        private static UserInfoDTO ToUserInfo(UserEntity u) => new UserInfoDTO
        {
            Id        = u.Id,
            Username  = u.Username,
            Email     = u.Email,
            FirstName = u.FirstName,
            LastName  = u.LastName,
            Role      = u.Role,
            IsActive  = u.IsActive
        };
    }
}