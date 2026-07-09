using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class FriendShip
    {
        
        public int FriendShipId { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
        // istek durumu
        public FriendShipStatus Status { get; set; } = FriendShipStatus.Pending;
        //istek tarihi
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
