using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Services
{
    public interface IFriendApiService
    {
        Task<List<UserSearchResultViewModel>> SearchUsersAsync(string username);
        Task<bool> SendRequestAsync(int receiverId);
        Task<List<PendingRequestViewModel>> GetPendingRequestsAsync();
        Task<bool> RespondToRequestAsync(int friendShipId, int isAccepted);
    }
}
