using AutoMapper;
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
    [Authorize]
    public class FriendShipController : ControllerBase
    {
        private readonly FriendShipService _friendshipService;
        private readonly ApiContext _context;


        public FriendShipController(ApiContext context, IMapper mapper)
        {
            _context = context;
            _friendshipService = new FriendShipService(_context, mapper);
        }

        private string? GetUserID()
        {
            return User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "UserId" || c.Type == "sub")?.Value;
        }

        [HttpPost("send-request")]
        public IActionResult SendRequest([FromBody] SendFriendRequestDto dto)
        {
            var userId = GetUserID();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            dto.SenderId = int.Parse(userId);
            var result = _friendshipService.SendRequest(dto);
            if (!result) return BadRequest("İstek Gönderilemedi. Geçersiz Kullanıcı veya zaten bir istek/arkadaşlık mevcut.");

            return Ok("Arkadaşlık isteği başarıyla gönderildi.");
        }

        [HttpPost("respond-request")]
        public IActionResult respondRequest([FromBody] FriendRequestActionDto dto)
        {
            var userId = GetUserID();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = _friendshipService.RespondToRequest(dto, int.Parse(userId));
            if (!result) return BadRequest("İstek bulunamadı veya işlem başarısız.");

            return Ok("Yanıtınız kaydedildi.");
        }

        [HttpGet("pending")]
        public IActionResult GetPendingRequests()
        {
            var userId = GetUserID();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(_friendshipService.GetPendingRequests(int.Parse(userId)));
        }

        [HttpGet("friends")]
        public IActionResult GetFriends()
        {
            var userId = GetUserID();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(_friendshipService.GetFriends(int.Parse(userId)));
        }
        [HttpGet("pending-count")]
        public IActionResult GetPendingCount()
        {
            var userId = GetUserID();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var parsedUserId = int.Parse(userId);
            var pendingFriendsCount = _friendshipService.GetPendingRequests(parsedUserId).Count();

            var pendingServerInvitesCount = _context.ServerInvitations
                .Count(si => si.ReceiverId == parsedUserId && si.Status == 0); 


            return Ok(new { count = pendingFriendsCount + pendingServerInvitesCount });
        }
    }
}
