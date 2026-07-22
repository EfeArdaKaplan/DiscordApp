using Business.Abstract;
using Business.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IWebHostEnvironment _env;

        public MessageController(IMessageService messageService, IWebHostEnvironment webHostEnvironment)
        {
            _messageService = messageService;
            _env = webHostEnvironment;
        }

        private string? GetUserId()
        {
            return User.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier || c.Type == "id" || c.Type == "UserId" || c.Type == "sub")?.Value;
        }

        [HttpPost("SendDirect")]
        public async Task<IActionResult> SendDirect([FromBody] SendMessageDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _messageService.SendDirectMessageAsync(int.Parse(userId), dto.ReceiverId, dto.Content);
            if (result) return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Mesaj gönderilemedi (arkadaş değilsiniz ya da mesaj boş)" });
        }

        [HttpGet("Conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _messageService.GetConversationAsync(int.Parse(userId), otherUserId);
            return Ok(messages);
        }

        [HttpPost("SendChannel")]
        public async Task<IActionResult> SendChannel([FromBody] SendChannelMessageDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _messageService.SendChannelMessageAsync(int.Parse(userId), dto.ChannelId, dto.Content);
            if (result) return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Mesaj gönderilemedi (bu sunucunun üyesi değilsin ya da mesaj boş!" });
        }

        [HttpGet("ChannelMessages/{channelId}")]
        public async Task<IActionResult> GetChannelMessages(int channelId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _messageService.GetChannelMessagesAsync(channelId, int.Parse(userId));
            if (messages == null) return Forbid();

            return Ok(messages);
        }

        [HttpPost("SendVoice")]
        [RequestSizeLimit(5_000_000)] // 5mb tavan ses dosyası sınırı koyyuok
        public async Task<IActionResult> SendVoice([FromForm] SendVoiceRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (request.Audio == null || request.Audio.Length == 0) 
                return BadRequest(new { success = false, message = "ses kaydı boş" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "voice");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}.webm";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Audio.CopyToAsync(stream);
            }

            var audioUrl = $"/uploads/voice/{fileName}";

            var dto = await _messageService.SendVoiceDirectMessageAsync(int.Parse(userId), request.ReceiverId, audioUrl, request.DurationSeconds);
            if (dto == null) return BadRequest(new { success = false, message = "mesaj gönderilemedi" });

            return Ok(new { success = true, message = dto });
        }

        public class SendVoiceRequest
        {
            public int ReceiverId { get; set; }
            public int DurationSeconds { get; set; }
            public IFormFile Audio { get; set; } = null;
        }
    }
}
