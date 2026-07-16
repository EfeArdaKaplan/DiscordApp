using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Abstract
{
    public interface IMessageApiService
    {
        Task<bool> SendMessageAsync(int receiverId, string content);
        Task<List<MessageViewModel>> GetConversationAsync(int otherUserId);
    }
}
