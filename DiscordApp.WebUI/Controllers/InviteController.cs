using DiscordApp.WebUI.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebUI.Controllers
{
    [Authorize]
    public class InviteController : Controller
    {
        private readonly IChannelApiService _channelApiService;
        public InviteController(IChannelApiService channelApiService) => _channelApiService = channelApiService;

        [HttpGet("/invite/{code}")]
        public async Task<IActionResult> Join(string code)
        {
            var (success, message, _) = await _channelApiService.JoinByInviteAsync(code);
            TempData["InviteMessage"] = success ? "Sunucuya katıldın! 🎉" : (message ?? "Davet linki geçersiz.");
            return RedirectToAction("Index", "Home");
        }
    }
}
