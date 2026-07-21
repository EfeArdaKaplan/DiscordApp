using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Abstract
{
    public interface IMessageApiService
    {
        Task<bool> SendMessageAsync(int receiverId, string content);
        Task<List<MessageViewModel>> GetConversationAsync(int otherUserId);

        Task<bool> SendChannelMessageAsync(int channelId, string content);
        Task<List<MessageViewModel>> GetChannelMessagesAsync(int channelId);
    }
}
