using Business.Abstract;
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
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(ApiContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _authService = new AuthService(_context);
            _jwtTokenService = jwtTokenService;
        }
        // POST: api/Auth/register      
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            var result = _authService.Register(dto);

            if (!result)
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

            var token = _jwtTokenService.GenerateToken(user, new List<string>());

            return Ok(new
            {
                Message = true,
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Token = token
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