using Business.Abstract;
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
    public class ChannelController : ControllerBase
    {
        private readonly IServerService _serverService;

        public ChannelController(IServerService serverService)
        {
            _serverService = serverService;
        }
        private string? GetUserId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "UserId" || c.Type == "sub")?.Value;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateChannelDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var channel = await _serverService.CreateChannelAsync(dto.ServerId, dto.Name, userId);
            if (channel != null)
                return Ok(new { success = true, channel });

            return BadRequest(new { success = false, message = "Kanal oluşturulamadı!" });
        }
    }

    public class CreateChannelDto
    {
        public int ServerId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
