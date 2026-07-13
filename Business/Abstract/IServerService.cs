using Business.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IServerService
    {
        Task<List<ServerDto>> GetUserServersAsync(string userId);
        Task<bool> CreateServerAsync(string serverName, string userId);
        Task<bool> CreateChannelAsync(int serverId, string channelName, string userId);
        Task<List<MemberDto>> GetServerMembersAsync(int serverId, string requestingUserId);
        /*
        Task<bool> DeleteServerAsync(int serverId, string userId);
        Task<bool> RenameServerAsync(int serverId, string newName, string userId);
        Task<bool> DeleteChannelAsync(int channelId, string userId);
        Task<bool> RenameChannelAsync(int channelId, string newName, string userId);
        /*/
    }
}