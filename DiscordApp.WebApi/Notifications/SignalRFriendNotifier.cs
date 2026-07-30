using Business.Abstract;
using DiscordApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DiscordApp.WebApi.Notifications
{
    public class SignalRFriendNotifier : IFriendNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;
        public SignalRFriendNotifier(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

        public async Task NotifyPendingCountChangedAsync(int userId)
        {
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("PendingCountChanged");
        }
    }
}