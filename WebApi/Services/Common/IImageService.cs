namespace WebApi.Services.Common;

public interface IImageService
{
    Task<(string url, byte[] resizedImage)> ProcessImageAsync(IFormFile file);
}