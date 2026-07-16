using Business.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IMessageService
    {
        Task<bool> SendDirectMessageAsync(int senderId, int receiverId, string content);
        Task<List<MessageDto>> GetConversationAsync(int userId, int otherUserId);
    }
}
