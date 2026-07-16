using DiscordApp.WebUI.Models;
using DiscordApp.WebUI.Services;

namespace DiscordApp.WebUI.Abstract
{
    public interface IAuthApiService
    {
        Task<ServiceResult<LoginResponseViewModel>> LoginAsync(LoginViewModel model);
        Task<ServiceResult<string>> RegisterAsync(RegisterViewModel model);
    }
}
