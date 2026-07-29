namespace DiscordApp.WebApi.Storage
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string bucketName);
    }
}