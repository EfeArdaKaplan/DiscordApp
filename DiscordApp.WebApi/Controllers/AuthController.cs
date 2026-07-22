using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Abstract;
using Business.Dtos;
using Business.Services;
using Data.Context;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMapper _mapper;

        public AuthController(ApiContext context, IJwtTokenService jwtTokenService, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _authService = new AuthService(_context,mapper);
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
        //[HttpGet("users")]
        //public IActionResult GetAllUsers()
        //{
        //    var users = _authService.GetAllUsers().Select(u => new { u.Id, u.UserName });
        //    return Ok(users);
        //}

        [HttpGet("search")]
        [Authorize]
        public IActionResult SearchUsers(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Ok(new List<object>());

            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "sub")?.Value;

            var results = _context.Users
                .Where(u => u.UserName != null && u.UserName.Contains(username) && u.Id.ToString() != currentUserId)
                .ProjectTo<UserSummaryDto>(_mapper.ConfigurationProvider)
                .Take(10)
                .ToList();

            return Ok(results);
        }
    }
}