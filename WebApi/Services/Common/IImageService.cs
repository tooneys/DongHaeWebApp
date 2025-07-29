namespace WebApi.Services.Common;

public interface IImageService
{
    Task<(string url, byte[] resizedImage)> ProcessImageAsync(IFormFile file);

    Task<(string url, byte[] resizedImage)> ProcessImageAsync(IFormFile file, string subFolder);

    Task<bool> DeleteImageAsync(string? path);
}