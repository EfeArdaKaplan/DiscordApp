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

        public async Task<List<ServerViewModel>> GetUserServersAsync()
        {
            try
            {
                var token = GetToken();
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Server/UserServers");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<ServerViewModel>>();
                    return result ?? new List<ServerViewModel>();
                }

                return new List<ServerViewModel>();
            }
            catch
            {
                return new List<ServerViewModel>();
            }
        }

        public async Task<bool> CreateServerAsync(string serverName)
        {
            try
            {
                var token = GetToken();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Server/Create")
                {
                    Content = JsonContent.Create(new { Name = serverName })
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateChannelAsync(int serverId, string channelName)
        {
            try
            {
                var token = GetToken();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Channel/Create")
                {
                    Content = JsonContent.Create(new { ServerId = serverId, Name = channelName })
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}