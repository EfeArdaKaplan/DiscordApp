using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordApp.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private string? GetUserId()
        {
            return Context.User?.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                // herkullanıcıyı kedni idsiyle aynı isimde gruba koyuyoruz
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }
    }
}
