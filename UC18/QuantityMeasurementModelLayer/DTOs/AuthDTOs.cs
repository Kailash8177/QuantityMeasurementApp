using System;
using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementModelLayer.DTOs
{
    public class RegisterRequestDTO
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginRequestDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public bool    Success      { get; set; }
        public string  Message      { get; set; } = string.Empty;
        public string? AccessToken  { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt  { get; set; }
        public UserInfoDTO? User    { get; set; }
    }

    public class UserInfoDTO
    {
        public long   Id        { get; set; }
        public string Username  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Role      { get; set; } = string.Empty;
        public bool   IsActive  { get; set; }
    }

    public class LogoutRequestDTO
    {
        public string? RefreshToken { get; set; }
    }

    public class UpdateRoleRequestDTO
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
