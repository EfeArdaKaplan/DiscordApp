using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int? ReceiverId { get; set; }
        public User Receiver { get; set; }

        public int? ChannelId { get; set; }
        public Channel Channel { get; set; }
    }
}
