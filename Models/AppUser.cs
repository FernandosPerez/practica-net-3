using Microsoft.AspNetCore.Identity;

namespace leccion_03_jwt.Models
{
    public class AppUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
    }
}