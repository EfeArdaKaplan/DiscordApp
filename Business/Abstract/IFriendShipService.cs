using Business.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IFriendShipService
    {
        Task<List<ServerDto>> GetUserServersAsync(string userId);
        Task<bool> CreateServerAsync(string serverName, string userId);
        Task<ChannelDto?> CreateChannelAsync(int serverId, string channelName, string userId);
        Task<List<MemberDto>> GetServerMembersAsync(int serverId, string requestingUserId);
        Task<string> GetInviteLinkAsync(int serverId, string userId);
        Task<JoinByInviteResultDto> JoinByInviteCodeAsync(string inviteCode, string userId);
        Task<bool> SendServerInvitationAsync(SendServerInvitationDto dto, string senderUserId);
        Task<bool> RespondServerInvitationAsync(ServerInvitationActionDto dto, string receiverUserId);
        Task<List<PendingServerInvitationDto>> GetPendingServerInvitationsAsync(string userId);
        Task<bool> IsMemberAsync(int serverId, int userId);
        Task<bool> IsChannelMemberAsync(int channelId, int userId);
    }
}
