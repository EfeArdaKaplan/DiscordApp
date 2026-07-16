using Business.Abstract;
using Business.Dtos;
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

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
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
    }
}
