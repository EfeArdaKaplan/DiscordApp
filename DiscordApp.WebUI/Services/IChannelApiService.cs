using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Services
{
    public interface IChannelApiService
    {
        Task<List<ServerViewModel>> GetUserServersAsync();
        Task<bool> CreateServerAsync(string serverName);
        Task<bool> CreateChannelAsync(int serverId, string channelName);
    }
}
