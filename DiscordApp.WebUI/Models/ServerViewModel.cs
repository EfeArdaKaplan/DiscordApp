namespace DiscordApp.WebUI.Models
{
    public class ServerViewModel
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public string? IconUrl { get; set; }
        public List<ChannelViewModel> Channels { get; set; } = new();
    }

    public class ChannelViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ServerId { get; set; }
        public string Type { get; set; } = "Text";
    }

    public class MemberViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
    }
}
