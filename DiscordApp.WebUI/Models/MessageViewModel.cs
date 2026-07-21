namespace DiscordApp.WebUI.Models
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int? ChannelId { get; set; }
    }
}
