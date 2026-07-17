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
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var servers = await _serverService.GetUserServersAsync(userId);
            return Ok(servers);
        }

        [HttpGet("{serverId}/Members")]
        public async Task<IActionResult> GetMembers(int serverId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var members = await _serverService.GetServerMembersAsync(serverId, userId);
            return Ok(members);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateServerDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token var ama içinden ID okunamadı!" });

            var result = await _serverService.CreateServerAsync(dto.Name, userId);
            if (result)
                return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Sunucu oluşturulamadı!" });
        }

        // JWT içindeki kullanıcı ID'sini hangi claim adıyla kaydedilmiş olursa olsun bulur
        private string? GetUserId()
        {
            return User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "id" ||
                c.Type == "UserId" ||
                c.Type == "sub")?.Value;
        }

        [HttpGet("{serverId}/InviteLink")]
        public async Task<IActionResult> GetInviteLink (int serverId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var code = await _serverService.GetInviteLinkAsync(serverId, userId);
            if (string.IsNullOrEmpty(code)) return BadRequest(new { success = false, message = "Bu sunucunun üyesi değilsin" });

            return Ok(new { success = true, inviteCode = code });
        }

        [HttpPost("JoinByInvite")]
        public async Task<IActionResult> JoinByInvite([FromBody] JoinByInviteDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty (userId)) return Unauthorized();

            var result = await _serverService.JoinByInviteCodeAsync(dto.InviteCode, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("InviteFriend")]
        public async Task<IActionResult> InviteFriend([FromBody] SendServerInvitationDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _serverService.SendServerInvitationAsync(dto, userId);
            if (!result) return BadRequest(new { success = false, message = "Davet Gönderilemedi (zaten üye/davetli" });
            return Ok(new { success = true });
        }

        [HttpPost("RespondInvite")]
        public async Task<IActionResult> RespondInvite([FromBody] ServerInvitationActionDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _serverService.RespondServerInvitationAsync(dto, userId);
            if (!result) return BadRequest(new { success = false, message = "İşlem başarısız" });
            return Ok(new { success = true });
        }

        [HttpGet("PendingInvites")]
        public async Task<IActionResult> PendingInvites()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(await _serverService.GetPendingServerInvitationsAsync(userId));
        }
    }
}
