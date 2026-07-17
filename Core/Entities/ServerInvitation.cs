using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class ServerInvitation
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;
        public int SenderId { get; set; }
        public AppUser Sender { get; set; } = null!;
        public int ReceiverId { get; set; }
        public AppUser Receiver { get; set; } = null!;
        public FriendShipStatus Status { get; set; } = FriendShipStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
