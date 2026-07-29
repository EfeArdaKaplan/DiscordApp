using Business.Abstract;
using Business.Dtos;
using DiscordApp.WebApi.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IFileStorageService _fileStorage;
        private readonly IConfiguration _config;

        public MessageController(IMessageService messageService, IFileStorageService fileStorage, IConfiguration config)
        {
            _messageService = messageService;
            _fileStorage = fileStorage;
            _config = config;
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
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> SendVoice([FromForm] SendVoiceRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (request.Audio == null || request.Audio.Length == 0)
                return BadRequest(new { success = false, message = "ses kaydı boş" });

            if (request.ReceiverId == null && request.ChannelId == null)
                return BadRequest(new { success = false, message = "Alıcı ya da kanal belirtilmedi" });

            var fileName = $"{Guid.NewGuid()}.webm";
            var voiceBucket = _config["Minio:VoiceBucket"]!;
            var audioUrl = await _fileStorage.UploadAsync(request.Audio.OpenReadStream(), fileName, "audio/webm", voiceBucket);

            MessageDto? dto;
            if (request.ChannelId.HasValue)
            {
                dto = await _messageService.SendVoiceChannelMessageAsync(int.Parse(userId), request.ChannelId.Value, audioUrl, request.DurationSeconds);
            }
            else
            {
                dto = await _messageService.SendVoiceDirectMessageAsync(int.Parse(userId), request.ReceiverId!.Value, audioUrl, request.DurationSeconds);
            }

            if (dto == null) return BadRequest(new { success = false, message = "mesaj gönderilemedi" });

            return Ok(new { success = true, message = dto });
        }

        public class SendVoiceRequest
        {
            public int? ReceiverId { get; set; }
            public int? ChannelId { get; set; }
            public int DurationSeconds { get; set; }
            public IFormFile? Audio { get; set; } = null;
        }
    }
}