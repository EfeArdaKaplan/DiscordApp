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
    }
}