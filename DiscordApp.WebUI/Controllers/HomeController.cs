using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DiscordApp.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IChannelApiService _channelApiService;
        private readonly IFriendApiService _friendApiService;
        private readonly IMessageApiService _messageApiService;

        public HomeController(IChannelApiService channelApiService, IFriendApiService friendApiService, IMessageApiService messageApiService)
        {
            _channelApiService = channelApiService;
            _friendApiService = friendApiService;
            _messageApiService = messageApiService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["UserName"] = User.Identity?.Name; // Auth cookie'sindeki ClaimTypes.Name
            var servers = await _channelApiService.GetUserServersAsync();
            var friends = await _friendApiService.GetFriendsAsync();
            ViewBag.Friends = friends;
            return View(servers);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var pendingFriends = await _friendApiService.GetPendingRequestsAsync();
                int friendCount = pendingFriends?.Count() ?? 0;

                var pendingInvites = await _channelApiService.GetPendingServerInvitesAsync();
                int inviteCount = pendingInvites?.Count() ?? 0;

                return Json(new { count = friendCount + inviteCount });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Bildirim sayısı alınırken hata: {ex.Message}");
                return Json(new { count = 0 });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> CreateServer([FromBody] CreateServerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(new { success = false, message = "Sunucu adı boş olamaz!" });

            var result = await _channelApiService.CreateServerAsync(request.Name);

            if (result)
                return Ok(new { success = true }); 

            return BadRequest(new { success = false, message = "Sunucu Oluşturulamadı" });
        }

        [HttpGet]
        public async Task<IActionResult> GetInviteLink(int serverId)
        {
            var code = await _channelApiService.GetInviteLinkAsync(serverId);
            if (string.IsNullOrEmpty(code)) return Json(new { success = false, message = "Davet linki alınamadı" });
            return Json(new { success = true, inviteCode = code });
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers(int serverId)
        {
            var members = await _channelApiService.GetServerMembersAsync(serverId);
            return Ok(members);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return Json(new { success = false, message = "Kanal adı boş olamaz!" });

            var channel = await _channelApiService.CreateChannelAsync(request.ServerId, request.Name, request.Type);
            if (channel != null)
                return Json(new { success = true, channel });

            return Json(new { success = false, message = "Kanal oluşturulamadı!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var messages = await _messageApiService.GetConversationAsync(otherUserId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Content))
                return BadRequest(new { success = false, message = "Mesaj boş olamaz!" });

            var result = await _messageApiService.SendMessageAsync(request.ReceiverId, request.Content);
            if (result) return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Mesaj gönderilemedi!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetChannelMessages(int channelId)
        {
            var messages = await _messageApiService.GetChannelMessagesAsync(channelId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendChannelMessage([FromBody] SendChannelMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Content))
                return BadRequest(new { succcess = false, message = "Mesaj boş olamaz!" });

            var result = await _messageApiService.SendChannelMessageAsync(request.ChannelId, request.Content);
            if (result) return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Mesaj gönderilemedi!" });
        }

        public class SendChannelMessageRequest
        {
            public int ChannelId { get; set; }
            public string Content { get; set; } = string.Empty;
        }
        public class SendMessageRequest
        {
            public int ReceiverId { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        public class CreateServerRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        public class CreateChannelRequest
        {
            public int ServerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "Text";
        }
    }
}