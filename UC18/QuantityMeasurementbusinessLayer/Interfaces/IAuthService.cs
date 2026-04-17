using QuantityMeasurementModelLayer.DTOs;

namespace QuantityMeasurementbusinessLayer.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, string ipAddress);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, string ipAddress);
        Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<AuthResponseDTO> LogoutAsync(string? refreshToken, long userId, string ipAddress);
        Task<UserInfoDTO?>    GetUserProfileAsync(long userId);
    }
}
