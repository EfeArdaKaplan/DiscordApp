using DiscordApp.WebUI.Models;
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

// 4. AuthApiService & HttpClient Kaydı
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    // WebAPI projenin çalıştığı port ve adresi buraya tanımlıyoruz
    client.BaseAddress = new Uri("https://localhost:7072/");
});

builder.Services.AddHttpClient<IChannelApiService, ChannelApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7072");
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
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();