using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class SendMessageDto
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class SendChannelMessageDto
    {
        public int ChannelId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class MessageDto
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int? ChannelId { get; set; }

        public string Type { get; set; } = "text";
        public string? AudioUrl { get; set; }
        public int? AudioDurationSeconds { get; set; }
    }
}
