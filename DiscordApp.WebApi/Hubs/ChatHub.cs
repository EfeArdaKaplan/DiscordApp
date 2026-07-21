using Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordApp.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IServerService _serverService;
        public ChatHub(IServerService serverService) => _serverService = serverService;
        private string? GetUserId()
        {
            return Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
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

        public async Task JoinChannel(int channelId)
        {
            var userId = GetUserId();
            if (userId == null || !int.TryParse(userId, out var uid)) return;

            if (!await _serverService.IsChannelMemberAsync(channelId, uid))
                throw new HubException("Bu kanala erişim yetkin yok");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel-{channelId}");
        }

        public async Task LeaveChannel(int channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel-{channelId}");
        }
    }
}
