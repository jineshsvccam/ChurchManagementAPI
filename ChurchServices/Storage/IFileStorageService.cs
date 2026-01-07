namespace ChurchServices.Storage
{
    public interface IFileStorageService
    {
        Task<string> GenerateUploadUrlAsync(string fileKey, string contentType);
        Task<string> GenerateDownloadUrlAsync(string fileKey);
    }
}
