using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DiscordApp.WebUI.Services
{
    public class ChannelApiService : IChannelApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChannelApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWToken")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst("Token")?.Value;

            // Eğer token'ın içinde yanlışlıkla "Bearer " kelimesi kaldıysa onu temizler
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(7).Trim();
            }

            return token;
        }

        // Tüm API çağrılarının ortak iskeleti: token ekleme, gönderme, JSON'a çevirme, hata yutma.
        private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);
                if (body != null) request.Content = JsonContent.Create(body);

                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return default;

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
                return default;
            }
        }

        // Sadece başarı/başarısızlık dönen (gövdesiz) POST istekleri için kısayol.
        private async Task<bool> PostSuccessAsync(string url, object? body = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                if (body != null) request.Content = JsonContent.Create(body);

                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ServerViewModel>> GetUserServersAsync()
        {
            var result = await SendAsync<List<ServerViewModel>>(HttpMethod.Get, "api/Server/UserServers");
            return result ?? new List<ServerViewModel>();
        }

        public async Task<List<MemberViewModel>> GetServerMembersAsync(int serverId)
        {
            var result = await SendAsync<List<MemberViewModel>>(HttpMethod.Get, $"api/Server/{serverId}/Members");
            return result ?? new List<MemberViewModel>();
        }

        public async Task<bool> CreateServerAsync(string serverName)
        {
            return await PostSuccessAsync("api/Server/Create", new { Name = serverName });
        }

        public async Task<ChannelViewModel?> CreateChannelAsync(int serverId, string channelName)
        {
            var result = await SendAsync<CreateChannelResponse>(HttpMethod.Post, "api/Channel/Create", new { ServerId = serverId, Name = channelName });
            return result?.Channel;
        }

        private class CreateChannelResponse { public bool Success { get; set; } public ChannelViewModel? Channel { get; set; } }

        public async Task<string?> GetInviteLinkAsync(int serverId)
        {
            var result = await SendAsync<InviteLinkResponse>(HttpMethod.Get, $"api/Server/{serverId}/InviteLink");
            return result?.InviteCode;
        }

        public async Task<(bool Success, string? Message, int? ServerId)> JoinByInviteAsync(string inviteCode)
        {
            var result = await SendAsync<JoinByInviteResponse>(HttpMethod.Post, "api/Server/JoinByInvite", new { InviteCode = inviteCode });
            return (result?.Success ?? false, result?.Message, result?.ServerId);
        }

        public async Task<bool> InviteFriendToServerAsync(int serverId, int receiverId)
        {
            return await PostSuccessAsync("api/Server/InviteFriend", new { ServerId = serverId, ReceiverId = receiverId });
        }

        public async Task<List<PendingServerInvitationViewModel>> GetPendingServerInvitesAsync()
        {
            var result = await SendAsync<List<PendingServerInvitationViewModel>>(HttpMethod.Get, "api/Server/PendingInvites");
            return result ?? new List<PendingServerInvitationViewModel>();
        }

        public async Task<bool> RespondServerInviteAsync(int invitationId, int isAccepted)
        {
            return await PostSuccessAsync("api/Server/RespondInvite", new { InvitationId = invitationId, IsAccepted = isAccepted });
        }

        private class InviteLinkResponse { public bool Success { get; set; } public string? InviteCode { get; set; } }
        private class JoinByInviteResponse { public bool Success { get; set; } public string? Message { get; set; } public int? ServerId { get; set; } }
    }
}