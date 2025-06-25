using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace WebApi.Services.Common;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;
    private const int MaxWidth = 800;
    private const int Quality = 80;

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<(string url, byte[] resizedImage)> ProcessImageAsync(IFormFile file)
    {
        try
        {
            _logger.LogInformation("이미지 처리 시작: FileName={FileName}, Size={Size}", 
                file.FileName, file.Length);

            // 1. 파일 유효성 검사
            if (file.Length == 0) 
                throw new ArgumentException("빈 파일입니다.");
            
            if (file.Length > 10 * 1024 * 1024) 
                throw new ArgumentException("파일 크기가 10MB를 초과합니다.");

            // 2. 안전한 웹 루트 경로 설정
            string webRootPath = _env.WebRootPath ?? Path.GetFullPath("wwwroot");
            string uploadFolder = Path.Combine(webRootPath, "uploads", "promotions");
            
            // 3. 디렉토리 존재 확인 및 생성
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
                _logger.LogInformation("업로드 폴더 생성: {UploadFolder}", uploadFolder);
            }

            // 4. 이미지 리사이징
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var imageBytes = stream.ToArray();

            using var image = Image.Load(imageBytes);
            
            // 비율 유지하며 리사이징
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(MaxWidth, 0),
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(resizeOptions));

            // 5. 품질 조정하여 저장
            var encoder = new JpegEncoder { Quality = Quality };
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, encoder);
            var resizedBytes = outputStream.ToArray();

            // 6. 안전한 파일 저장
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, resizedBytes);

            var imageUrl = $"/uploads/promotions/{fileName}";

            _logger.LogInformation("이미지 처리 완료: OriginalSize={OriginalSize}, ResizedSize={ResizedSize}, Url={Url}", 
                file.Length, resizedBytes.Length, imageUrl);

            return (imageUrl, resizedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "이미지 처리 중 오류 발생: FileName={FileName}", file.FileName);
            throw;
        }
    }
}