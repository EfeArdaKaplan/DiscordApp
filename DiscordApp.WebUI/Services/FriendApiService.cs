using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using System.Net.Http.Headers;

namespace DiscordApp.WebUI.Services
{
    public class FriendApiService : IFriendApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FriendApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // generic istek gönderme methodu

        private async Task<HttpResponseMessage?> SendAsync(HttpMethod method, string url, object? body = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);
                if (body != null) request.Content = JsonContent.Create(body);

                var token = _httpContextAccessor.HttpContext?.Items["JwtToken"] as string;
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await _httpClient.SendAsync(request);
            }
            catch { return null; }
        }

        public async Task<List<UserSearchResultViewModel>> SearchUsersAsync(string username)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/Auth/search?username={username}");
            if (response == null || !response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<List<UserSearchResultViewModel>>() ?? new();
        }
        public async Task<bool> SendRequestAsync(int receiverId)
        {
            var response = await SendAsync(HttpMethod.Post, "api/FriendShip/send-request", new { ReceiverId = receiverId });
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<List<PendingRequestViewModel>> GetPendingRequestsAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/FriendShip/pending");
            if (response == null || !response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<List<PendingRequestViewModel>>() ?? new();
        }

        public async Task<bool> RespondToRequestAsync(int friendShipId, int isAccepted)
        {
            var response = await SendAsync(HttpMethod.Post, "api/friendShip/respond-request", new { FriendShipId = friendShipId, IsAccepted =isAccepted });
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<List<UserSearchResultViewModel>> GetFriendsAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/FriendShip/friends");
            if (response == null || !response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<List<UserSearchResultViewModel>>() ?? new();
        }
    }
}
