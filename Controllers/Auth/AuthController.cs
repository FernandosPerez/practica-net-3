using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using leccion_03_jwt.DTOs;
using leccion_03_jwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace leccion_03_jwt.Controllers.Auth{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private UserManager<AppUser> _userManager ;
        private IConfiguration _config;
        public AuthController(UserManager<AppUser> userManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _config = configuration;       
        }
        

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegisterDto register)
        {
            var nuevo = new AppUser {Email = register.Email,UserName = register.Email,NombreCompleto = register.NombreCompleto};
            var registro = await _userManager.CreateAsync(nuevo, register.Password);

            if (!registro.Succeeded)
            {
                return BadRequest(registro.Errors);
            } 
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login( [FromBody] LoginDto login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest("Debes llenar todos los campos");
            }

            try
            {
                //Buscar al usuario en MySQL usando el UserManager y await porque es asincronoooo
                var usuario = await _userManager.FindByEmailAsync(login.Email);

                if (usuario == null)
                {
                    return Unauthorized(new { Message = "Credenciales incorrectas." }); //si no esta, regresamos la respuesta
                }

                //Verificar si la contraseña coincide (Identity se encarga de desencriptar el Hash)
                var passwordValido = await _userManager.CheckPasswordAsync(usuario, login.Password);

                if (!passwordValido)
                {
                    return Unauthorized(new { Message = "Credenciales incorrectas." });
                }

                var token = GenerarToken(usuario.Email, usuario.Id);
                return Ok(new { Token = token});
            }
            catch
            {
                return StatusCode(500, $"sin acceso al servicio, intentalo mas tarde");
            }

        } 

        //metodo privado poorque no se debe acceder por ningun endpoint esta funcion ya que es la que procesa y encripta los datos de manera interna
         private string GenerarToken (string email, string usuarioId)
        {
              // 1. Definir los Claims (Información del usuario que viajará en el token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
                new Claim(ClaimTypes.Role, "Usuario") // Rol opcional
            };

            // 2. Obtener la clave secreta (Debe venir de tu appsettings.json y ser larga)
            var claveSecreta = _config["Jwt:Key"];
            var llaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
            
            // 3. Configurar las credenciales de firma (Algoritmo de encriptación)
            var credenciales = new SigningCredentials(llaveSimetrica, SecurityAlgorithms.HmacSha256);

            // 4. Crear el JwtSecurityToken
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],                 // Quién emite el token
                audience: _config["Jwt:Audience"],                 // Para quién está destinado
                claims: claims,                           // Los datos del usuario
                expires: DateTime.UtcNow.AddHours(2),     // Tiempo de vida del token
                signingCredentials: credenciales          // Firma de seguridad
            );

            // 5. Serializar el token a una cadena de texto (String)
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }
    }
    
}