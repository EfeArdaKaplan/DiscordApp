using DiscordApp.WebUI.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebUI.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly IFriendApiService _friendApiService;
        private readonly IChannelApiService _channelApiService;

        public FriendController(IFriendApiService friendApiService, IChannelApiService channelApiService)
        {
            _friendApiService = friendApiService;
            _channelApiService = channelApiService;
        }

        [HttpGet]
        public async Task<IActionResult> MyServers()
        {
            var servers = await _channelApiService.GetUserServersAsync();
            return Json(servers.Select(s => new { s.Id, s.Name }));
        }

        [HttpPost]
        public async Task<IActionResult> InviteToServer([FromBody] InviteToServerModel model)
        {
            var result = await _channelApiService.InviteFriendToServerAsync(model.ServerId, model.ReceiverId);
            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Davet gönderilemedi (zaten üye ya da bekleyen davet var)" });
        }

        public class InviteToServerModel { public int ServerId { get; set; } public int ReceiverId { get; set; } }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string username)
        {
            var results = await _friendApiService.SearchUsersAsync(username);
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest([FromBody] SendRequestModel model)
        {
            var result = await _friendApiService.SendRequestAsync(model.ReceiverId);
            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "istek gönderilemedi (Zaten arkadaşsınız ya da istek bekliyor" });
        }

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var pending = await _friendApiService.GetPendingRequestsAsync();
            ViewBag.ServerInvites = await _channelApiService.GetPendingServerInvitesAsync();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> Respond([FromBody] RespondModel model)
        {
            var result = await _friendApiService.RespondToRequestAsync(model.FriendShipId, model.IsAccepted);
            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "İşlem başarısız" });
        }

        [HttpPost]
        public async Task<IActionResult> RespondServerInvite([FromBody] RespondServerInviteModel model)
        {
            var result = await _channelApiService.RespondServerInviteAsync(model.InvitationId, model.IsAccepted);
            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false });
        }

        public class RespondServerInviteModel { public int InvitationId { get; set; } public int IsAccepted { get; set; } }

        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            var pending = await _friendApiService.GetPendingRequestsAsync();
            return Json(new { count = pending.Count });
        }

        public class SendRequestModel { public int ReceiverId { get; set; } }
        public class RespondModel { public int FriendShipId { get; set; } public int IsAccepted { get; set; } }
    }
}
