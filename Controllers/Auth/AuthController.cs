using leccion_03_jwt.DTOs;
using leccion_03_jwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

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

                return Ok("login correcto");
            }
            catch
            {
                return StatusCode(500, $"sin acceso al servicio, intentalo mas tarde");
            }

        } 
    }
    
}