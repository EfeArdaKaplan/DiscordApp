using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Abstract
{
    public interface IChannelApiService
    {
        Task<List<ServerViewModel>> GetUserServersAsync();
        Task<bool> CreateServerAsync(string serverName);
        Task<bool> CreateChannelAsync(int serverId, string channelName);
        Task<List<MemberViewModel>> GetServerMembersAsync(int serverId);
        Task<string?> GetInviteLinkAsync(int serverId);
        Task<(bool Success, string? Message, int? ServerId)> JoinByInviteAsync(string inviteCode);
        Task<bool> InviteFriendToServerAsync(int serverId, int receiverId);
        Task<List<PendingServerInvitationViewModel>> GetPendingServerInvitesAsync();
        Task<bool> RespondServerInviteAsync(int invitationId, int isAccepted);
    }
}
