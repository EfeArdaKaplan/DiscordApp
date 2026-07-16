using DiscordApp.WebUI.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebUI.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly IFriendApiService _friendApiService;

        public FriendController(IFriendApiService friendApiService)
        {
            _friendApiService = friendApiService;
        }

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
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> Respond([FromBody] RespondModel model)
        {
            var result = await _friendApiService.RespondToRequestAsync(model.FriendShipId, model.IsAccepted);
            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "İşlem başarısız" });
        }

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
