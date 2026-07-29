using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using DiscordApp.WebUI.Security;
using DiscordApp.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller & Views
builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "App.AuthToken";
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout"; 
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// 3. HttpContext Accessor (Cookie ve Session erişimleri için)
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<TokenProtector>();

// 4. AuthApiService & HttpClient Kaydı
// API adresi TEK bir yerden okunuyor: appsettings.json -> ApiSettings:BaseUrl
// Cloudflare tunnel URL'i değiştikçe sadece o değeri güncellemek yeterli.
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl appsettings.json içinde tanımlı değil.");

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IChannelApiService, ChannelApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IFriendApiService, FriendApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMessageApiService, MessageApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<JwtAccessMiddleware>();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var path = context.Request.Path.Value ?? "";
        if (path.EndsWith(".js") || path.EndsWith(".css"))
        {
            var ct = context.Response.ContentType;
            if (!string.IsNullOrEmpty(ct) && !ct.Contains("charset"))
            {
                context.Response.ContentType = ct + "; charset=utf-8";
            }
        }
        return Task.CompletedTask;
    });
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();