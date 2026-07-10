namespace DiscordApp.WebUI.Services
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMEssage { get; set; }

        public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ServiceResult<T> Failure(string error) => new() {IsSuccess =false, ErrorMEssage = error};
    }
}
