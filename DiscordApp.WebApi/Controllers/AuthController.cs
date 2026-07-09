using Business.Dtos;
using Business.Services;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController()
        {
            var context = new ApiContext();
            _authService = new AuthService(context);
        }
        // POST: api/Auth/register      
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            var result = _authService.Register(dto);

            if(!result)
            {
                return BadRequest("Bu e-posta adresi zaten bir başkası tarafından kullanılıyor");
            }

            return Ok("Kullanıcı kaydı başarıyla oluşturuldu");
        }
        // POST: api/Auth/login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _authService.Login(dto);

            if (user == null)
            {
                return Unauthorized("E-posta adresi veya şifre hatalı!");
            }
            return Ok(new
            {
                Message = "Giriş Başarılı",
                UserId = user.UserId,
                Username = user.Username
            });
        }
    }
}
