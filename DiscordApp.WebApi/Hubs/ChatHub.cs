using Business.Abstract;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace DiscordApp.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IServerService _serverService;
        public ChatHub(IServerService serverService) => _serverService = serverService;

        // channelId -> (connectionId -> kullanıcı bilgisi)  -> odada kimin olduğunu bellekte tutuyoruz
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, VoiceUser>> _voiceChannelUsers = new();

        // connectionId -> channelId  -> bağlantı koptuğunda hangi odadan çıkaracağımızı bilmek için
        private static readonly ConcurrentDictionary<string, int> _connectionVoiceChannel = new();

        private string? GetUserId()
        {
            return Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUserName()
        {
            return Context.User?.Identity?.Name;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionVoiceChannel.TryRemove(Context.ConnectionId, out var channelId))
            {
                await RemoveFromVoiceChannelInternal(channelId, Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
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

        // ============ SESLİ KANAL ============

        public async Task<List<object>> JoinVoiceChannel(int channelId)
        {
            var userId = GetUserId();
            var userName = GetUserName();
            if (userId == null || userName == null || !int.TryParse(userId, out var uid)) return new List<object>();

            if (!await _serverService.IsChannelMemberAsync(channelId, uid))
                throw new HubException("Bu kanala erişim yetkin yok");

            if (_connectionVoiceChannel.TryGetValue(Context.ConnectionId, out var oldChannelId) && oldChannelId != channelId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"voice-{oldChannelId}");
                await RemoveFromVoiceChannelInternal(oldChannelId, Context.ConnectionId);
            }

            // Kendisi eklenmeden ÖNCEKİ mevcut katılımcı listesi -> bunlara offer gönderecek
            var existingPeers = _voiceChannelUsers.TryGetValue(channelId, out var currentUsers)
                ? currentUsers.Select(kvp => (object)new { ConnectionId = kvp.Key, kvp.Value.UserId, kvp.Value.UserName }).ToList()
                : new List<object>();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"voice-{channelId}");

            var users = _voiceChannelUsers.GetOrAdd(channelId, _ => new ConcurrentDictionary<string, VoiceUser>());
            users[Context.ConnectionId] = new VoiceUser { UserId = uid, UserName = userName };
            _connectionVoiceChannel[Context.ConnectionId] = channelId;

            await BroadcastVoiceUsers(channelId);

            return existingPeers;
        }

        public async Task LeaveVoiceChannel(int channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"voice-{channelId}");
            _connectionVoiceChannel.TryRemove(Context.ConnectionId, out _);
            await RemoveFromVoiceChannelInternal(channelId, Context.ConnectionId);
        }

        public async Task SendVoiceOffer(string targetConnectionId, string offerSdp)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveVoiceOffer", Context.ConnectionId, offerSdp);
        }

        public async Task SendVoiceAnswer(string targetConnectionId, string answerSdp)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveVoiceAnswer", Context.ConnectionId, answerSdp);
        }

        public async Task SendVoiceIceCandidate(string targetConnectionId, string candidateJson)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveVoiceIceCandidate", Context.ConnectionId, candidateJson);
        }

        private async Task RemoveFromVoiceChannelInternal(int channelId, string connectionId)
        {
            if (_voiceChannelUsers.TryGetValue(channelId, out var users))
            {
                users.TryRemove(connectionId, out _);
                if (users.IsEmpty)
                {
                    _voiceChannelUsers.TryRemove(channelId, out _);
                }
            }
            await BroadcastVoiceUsers(channelId);
        }

        private async Task BroadcastVoiceUsers(int channelId)
        {
            var list = _voiceChannelUsers.TryGetValue(channelId, out var users)
                ? users.Select(kvp => (object)new { ConnectionId = kvp.Key, kvp.Value.UserId, kvp.Value.UserName }).ToList() 
                : new List<object>();

            await Clients.Group($"voice-{channelId}").SendAsync("VoiceChannelUsersUpdated", channelId, list);
        }
        private class VoiceUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
        }
    }
}