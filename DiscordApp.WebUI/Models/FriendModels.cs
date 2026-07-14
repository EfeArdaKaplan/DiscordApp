namespace DiscordApp.WebUI.Models
{
    public class UserSearchResultViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class PendingRequestViewModel
    {
        public int FriendShipId { get; set; }
        public int SenderId { get; set; }
        public string SenderUserName{ get; set; } = string.Empty;
    }
}
