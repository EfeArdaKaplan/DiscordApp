using Business.Abstract;
using Business.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DiscordApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServerController : ControllerBase
    {
        private readonly IServerService _serverService;

        public ServerController(IServerService serverService)
        {
            _serverService = serverService;
        }

        [HttpGet("UserServers")]
        public async Task<IActionResult> GetUserServers()
        {
            // JWT içindeki ID'yi hangi isimle kaydedilmiş olursa olsun bulur
            var userId = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "id" ||
                c.Type == "UserId" ||
                c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var servers = await _serverService.GetUserServersAsync(userId);
            return Ok(servers);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateServerDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "id" ||
                c.Type == "UserId" ||
                c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var result = await _serverService.CreateServerAsync(dto.Name, userId);
            if (result)
                return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Sunucu oluşturulamadı!" });
        }
    }
}