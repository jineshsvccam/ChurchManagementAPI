using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace ChurchServices.Storage
{
    public class S3StorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly int _expiryMinutes;

        public S3StorageService(IConfiguration configuration)
        {
            _bucketName = configuration["AWS:BucketName"]
                ?? throw new ArgumentNullException("AWS:BucketName");

            _expiryMinutes = int.Parse(configuration["AWS:PresignExpiryMinutes"] ?? "10");

            var region = RegionEndpoint.GetBySystemName(
                configuration["AWS:Region"] ?? "ap-south-1"
            );

            _s3Client = new AmazonS3Client(region);
        }

        // 🔹 Upload (PUT)
        public async Task<string> GenerateUploadUrlAsync(string fileKey, string contentType)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                ContentType = contentType
            };

            return _s3Client.GetPreSignedURL(request);
        }

        // 🔹 Download (GET)
        public async Task<string> GenerateDownloadUrlAsync(string fileKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes)
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
