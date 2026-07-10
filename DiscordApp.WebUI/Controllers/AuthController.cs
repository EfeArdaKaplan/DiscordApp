using DiscordApp.WebUI.Models;
using DiscordApp.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiscordApp.WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public AuthController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        [HttpGet]
        public IActionResult Login () => View();
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            var result = await _authApiService.LoginAsync(vm);
            if (result.IsSuccess && result.Data != null && result.Data.Message == true)
            {
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
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = result.ErrorMEssage;
            return View(vm);
        }
    }
}
