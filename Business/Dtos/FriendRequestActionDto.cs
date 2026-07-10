using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class FriendRequestActionDto
    {
        public int FriendShipId { get; set; }
        public int IsAccepted { get; set; } // 1 = Accepted, 0 = Rejected
    }
}
