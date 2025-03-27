using Microsoft.AspNetCore.Http;

namespace Zap.DataAccess.Services;

public interface IFileUploadService
{
    Task<(string url, string key)> UploadAvatarAsync(IFormFile file);
    Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file);
    Task<(string url, string key)> UploadAttachmentAsync(IFormFile file);
    Task DeleteFileAsync(string key);
}