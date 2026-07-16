using Business.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IMessageNotifier
    {
        Task NotifyNewMessageAsync(int receiverId, MessageDto message);
    }
}
