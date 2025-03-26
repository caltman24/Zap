using Microsoft.AspNetCore.Http;

namespace Zap.DataAccess.Services;

public interface IFileUploadService
{
    Task<string> UploadAvatarAsync(IFormFile file);
    Task<string> UploadAttachmentAsync(IFormFile file);
}