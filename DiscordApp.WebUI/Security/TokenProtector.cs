using Microsoft.AspNetCore.DataProtection;

namespace DiscordApp.WebUI.Security
{
    public class TokenProtector
    {
        private readonly IDataProtector _protector;

        public TokenProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("DiscordApp.JwtProtection.v1");
        }

        public string Protect(string rawToken) => _protector.Protect(rawToken);
        public string? Unprotect(string protectedToken)
        {
            try { return _protector.Unprotect(protectedToken); }
            catch { return null; }
        }
    }
}
