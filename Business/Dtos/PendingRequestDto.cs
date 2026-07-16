using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class PendingRequestDto
    {
        public int FriendShipId { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName { get; set; } = string.Empty;
    }
}
