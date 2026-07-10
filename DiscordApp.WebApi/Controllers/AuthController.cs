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
        private readonly ApiContext _context;

        public AuthController(ApiContext context)
        {
            _context = context;
            _authService = new AuthService(_context);
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
                Message = true,
                UserId = user.Id,
                Username = user.UserName,
            });
        }
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _authService.GetAllUsers();
            return Ok(users);
        }
    }
}
