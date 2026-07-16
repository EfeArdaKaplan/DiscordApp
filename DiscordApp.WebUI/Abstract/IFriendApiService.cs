using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Abstract
{
    public interface IFriendApiService
    {
        Task<List<UserSearchResultViewModel>> SearchUsersAsync(string username);
        Task<bool> SendRequestAsync(int receiverId);
        Task<List<PendingRequestViewModel>> GetPendingRequestsAsync();
        Task<bool> RespondToRequestAsync(int friendShipId, int isAccepted);
        Task<List<UserSearchResultViewModel>> GetFriendsAsync();
    }
}
