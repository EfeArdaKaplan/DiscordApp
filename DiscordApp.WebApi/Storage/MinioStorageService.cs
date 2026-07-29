using Amazon.S3;
using Amazon.S3.Model;

namespace DiscordApp.WebApi.Storage
{
    public class MinioStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly string _publicEndpoint;

        public MinioStorageService(IConfiguration config)
        {
            var section = config.GetSection("Minio");
            _publicEndpoint = section["Endpoint"]!;

            _s3 = new AmazonS3Client(
                section["AccessKey"],
                section["SecretKey"],
                new AmazonS3Config
                {
                    ServiceURL = section["Endpoint"],
                    ForcePathStyle = true
                });
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string bucketName)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3.PutObjectAsync(request);

            return $"{_publicEndpoint}/{bucketName}/{fileName}";
        }
    }
}
