using DiscordApp.WebUI.Models;

namespace DiscordApp.WebUI.Services
{
    public interface IAuthApiService
    {
        Task<ServiceResult<LoginResponseViewModel>> LoginAsync(LoginViewModel model);
        Task<ServiceResult<string>> RegisterAsync(RegisterViewModel model);
    }
}
