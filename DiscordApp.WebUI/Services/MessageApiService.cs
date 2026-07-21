using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DiscordApp.WebUI.Services
{
    public class MessageApiService : IMessageApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<HttpResponseMessage?> SendAsync(HttpMethod method, string url, object? body = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);
                if (body != null) request.Content = JsonContent.Create(body);

                var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await _httpClient.SendAsync(request);
            }
            catch { return null; }
        }

        public async Task<bool> SendMessageAsync(int receiverId, string content)
        {
            var response = await SendAsync(HttpMethod.Post, "api/Message/SendDirect", new { ReceiverId = receiverId, Content = content });
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<List<MessageViewModel>> GetConversationAsync(int otherUserId)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/Message/Conversation/{otherUserId}");
            if (response == null || !response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<List<MessageViewModel>>() ?? new();
        }

        public async Task<bool> SendChannelMessageAsync(int channelId, string content)
        {
            var response = await SendAsync(HttpMethod.Post, "api/Message/SendChannel", new { ChannelId = channelId, Content = content });
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<List<MessageViewModel>> GetChannelMessagesAsync(int channelId)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/Message/ChannelMessages/{channelId}");
            if (response == null || !response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<List<MessageViewModel>>() ?? new();
        }
    }
}