using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DiscordApp.WebUI.Security
{
    public class JwtAccessMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtAccessMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, TokenProtector protector)
        {
            var encrypted = context.User?.FindFirst("JWToken")?.Value;

            if (!string.IsNullOrEmpty(encrypted))
            {
                var rawToken = protector.Unprotect(encrypted);

                if (rawToken != null )
                {
                    context.Items["JwtToken"] = rawToken;
                } else
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }

            await _next(context);
        }
    }
}
