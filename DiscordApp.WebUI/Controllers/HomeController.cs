using DiscordApp.WebUI.Models;
using DiscordApp.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DiscordApp.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IChannelApiService _channelApiService;

        public HomeController(IChannelApiService channelApiService)
        {
            _channelApiService = channelApiService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. API'den sunucuları çekiyoruz
            var servers = await _channelApiService.GetUserServersAsync();

            // 2. Çektiğimiz listeyi HTML ekranınaGÖNDERİYORUZ!
            return View(servers);
        }

        public IActionResult Privacy()
        {
            return View();
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
        public async Task<IActionResult> GetMembers(int serverId)
        {
            var members = await _channelApiService.GetServerMembersAsync(serverId);
            return Ok(members);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(new { success = false, message = "Kanal adı boş olamaz!" });

            var result = await _channelApiService.CreateChannelAsync(request.ServerId, request.Name);

            if (result)
                return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Kanal oluşturulamadı!" });
        }

        public class CreateServerRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        public class CreateChannelRequest
        {
            public int ServerId { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}