using Business.Dtos;
using Business.Services;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendShipController : ControllerBase
    {
        private readonly FriendShipService _friendshipService;
        private readonly ApiContext _context;


        public FriendShipController(ApiContext context)
        {
            _context = context;
            _friendshipService = new FriendShipService(_context);
        }

        [HttpPost("send-request")]
        public IActionResult SendRequest([FromBody] SendFriendRequestDto dto)
        {
            var result = _friendshipService.SendRequest(dto);
            if (!result) return BadRequest("İstek Gönderilemedi. Geçersiz Kullanıcı veya zaten bir istek/arkadaşlık mevcut.");

            return Ok("Arkadaşlık isteği başarıyla gönderildi.");
        }

        [HttpPost("respond-request")]
        public IActionResult respondRequest([FromBody] FriendRequestActionDto dto)
        {
            var result = _friendshipService.RespondToRequest(dto);
            if (!result) return BadRequest("İstek bulunamadı veya işlem başarısız.");

            return Ok("Yanıtınız kaydedildi.");
        }
        [HttpGet("friends/{userId}")]
        public IActionResult GetFriends(int userId)
        {
            var friends = _friendshipService.GetFriends(userId);
            return Ok(friends);
        }
    }
}
