using DiscordApp.WebUI.Abstract;
using DiscordApp.WebUI.Services;
using Newtonsoft.Json;
using System.Text;

namespace DiscordApp.WebUI.Models
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7072/api/Auth/";

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<LoginResponseViewModel>> LoginAsync(LoginViewModel model)
        {
            return await SendPostRequestAsync<LoginResponseViewModel>("Login", model);
        }

        public async Task<ServiceResult<string>> RegisterAsync(RegisterViewModel model)
        {
            return await SendPostRequestAsync<string>("Register", model);
        }

        // ================= GENERIC POST METODU =================
        private async Task<ServiceResult<TResponse>> SendPostRequestAsync<TResponse>(string endpoint, object model)
        {
            try
            {
                var jsonModel = JsonConvert.SerializeObject(model);
                var requestContent = new StringContent(jsonModel, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}{endpoint}", requestContent);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (typeof(TResponse) == typeof(string))
                    {
                        return ServiceResult<TResponse>.Success((TResponse)(object)content);
                    }

                    var data = JsonConvert.DeserializeObject<TResponse>(content);
                    return ServiceResult<TResponse>.Success(data!);
                }

                string cleanError = content.Replace("\"", "").Trim();
                if (string.IsNullOrWhiteSpace(cleanError))
                {
                    cleanError = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "E-posta veya şifre hatalı.",
                        System.Net.HttpStatusCode.BadRequest => "Geçersiz bir istek gönderildi.",
                        System.Net.HttpStatusCode.NotFound => "Aranan kaynak bulunamadı.",
                        _ => "Bir hata oluştu. Lütfen tekrar deneyin."
                    };
                }
                return ServiceResult<TResponse>.Failure(cleanError);
            }
            catch (Exception)
            {
                return ServiceResult<TResponse>.Failure("Kimlik doğrulama sunucusuna bağlanılamadı.");
            }
        }
    }
}