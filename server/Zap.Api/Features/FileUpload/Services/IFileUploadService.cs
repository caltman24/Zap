namespace Zap.Api.Features.FileUpload.Services;

public interface IFileUploadService
{
    /// <summary>
    /// Uploads a user avatar to the storage
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="oldKey">Old object key to remove</param>
    /// <returns>URL and key of the uploaded file</returns>
    Task<(string url, string key)> UploadAvatarAsync(IFormFile file, string? oldKey = null);
    /// <summary>
    /// Uploads a company logo to the storage
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="oldKey">Old object key to remove</param>
    /// <returns>URL and key of the uploaded file</returns>
    Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file, string? oldKey = null);
    Task<(string url, string key)> UploadAttachmentAsync(IFormFile file);
    Task DeleteFileAsync(string key);
}