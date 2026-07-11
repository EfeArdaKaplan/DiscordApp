namespace DiscordApp.WebUI.Models
{
    public class LoginResponseViewModel
    {
        public bool Message { get; set; }

        public string? Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
