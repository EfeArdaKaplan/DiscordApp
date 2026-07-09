using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class SendFriendRequestDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
    
}
