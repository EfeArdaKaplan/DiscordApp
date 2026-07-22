using Business.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IServerNotifier
    {
        Task NotifyChannelCreatedAsync(List<int> memberUserIds, ChannelDto channel);
    }
}