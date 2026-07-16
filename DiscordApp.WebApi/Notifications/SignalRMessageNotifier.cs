using Business.Abstract;
using Business.Dtos;
using DiscordApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DiscordApp.WebApi.Notifications
{
    public class SignalRMessageNotifier : IMessageNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRMessageNotifier(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewMessageAsync(int receiverId, MessageDto message)
        {
            await _hubContext.Clients.Group(receiverId.ToString())
                .SendAsync("receiveMessage", message);
        }
    }
}
