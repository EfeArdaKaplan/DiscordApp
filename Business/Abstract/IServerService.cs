using Business.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IServerService
    {
        Task<List<ServerDto>> GetUserServersAsync(string userId);
        Task<bool> CreateServerAsync(string serverName, string userId);
    }
}