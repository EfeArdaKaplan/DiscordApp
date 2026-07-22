using Business.Abstract;
using Business.Dtos;
using DiscordApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DiscordApp.WebApi.Notifications
{
    public class SignalRServerNotifier : IServerNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;
        public SignalRServerNotifier(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

        public async Task NotifyChannelCreatedAsync(List<int> memberUserIds, ChannelDto channel)
        {
            var groups = memberUserIds.Select(id => id.ToString()).ToList();
            await _hubContext.Clients.Groups(groups).SendAsync("ChannelCreated", channel);
        }
    }
}
