using System.ComponentModel.DataAnnotations;

namespace leccion_03_jwt.DTOs
{
    public class RegisterDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
    }
}