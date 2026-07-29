using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Models;
using DiscordApp.WebUI.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordApp.WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly TokenProtector _tokenProtector;

        public AuthController(IAuthApiService authApiService, TokenProtector tokenProtector)
        {
            _authApiService = authApiService;
            _tokenProtector = tokenProtector;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _authApiService.LoginAsync(vm);

            if (result.IsSuccess && result.Data != null && result.Data.Message == true)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.Data.UserId.ToString()),
                    new Claim(ClaimTypes.Name, result.Data.Username ?? string.Empty),
                    new Claim(ClaimTypes.Email, result.Data.Email ?? string.Empty),
                    new Claim("JWToken", _tokenProtector.Protect(result.Data.Token ?? string.Empty)) 
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Tarayıcı kapandığında oturum hatırlansın mı
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                // Kullanıcının tarayıcısına güvenli Cookie basıyoruz
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = result.ErrorMEssage ?? "E-posta veya şifre hatalı";
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            var result = await _authApiService.RegisterAsync(vm);

            if (result.IsSuccess)
            {
                return RedirectToAction("Login", "Auth"); 
            }

            ViewBag.ErrorMessage = result.ErrorMEssage;
            return View(vm);
        }

        // Kullanıcı Çıkış Yapmak İstediğinde Çalışacak Metot
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}