namespace WebApi.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string? fileUrl);
        bool IsValidFileType(IFormFile file, string[] allowedExtensions);
        bool IsValidFileSize(IFormFile file, long maxSizeInBytes);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileUploadService> _logger;

        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // 파일 유효성 검사
                var allowedExtensions = folder == "covers" ? _allowedImageExtensions : _allowedDocumentExtensions;

                if (!IsValidFileType(file, allowedExtensions))
                {
                    throw new ArgumentException($"지원하지 않는 파일 형식입니다. 허용된 확장자: {string.Join(", ", allowedExtensions)}");
                }

                if (!IsValidFileSize(file, _maxFileSize))
                {
                    throw new ArgumentException($"파일 크기가 너무 큽니다. 최대 크기: {_maxFileSize / (1024 * 1024)}MB");
                }

                // 업로드 폴더 생성
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);

                // 안전한 파일명 생성
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // 파일 저장
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // URL 생성
                var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5000";
                var fileUrl = $"{baseUrl}/uploads/{folder}/{fileName}";

                _logger.LogInformation("파일이 성공적으로 업로드되었습니다. URL: {FileUrl}", fileUrl);
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파일 업로드 중 오류가 발생했습니다. 파일명: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string? fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return true;

            try
            {
                var uri = new Uri(fileUrl);
                var relativePath = uri.AbsolutePath.TrimStart('/');
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("파일이 성공적으로 삭제되었습니다. 경로: {FilePath}", filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파일 삭제 중 오류가 발생했습니다. URL: {FileUrl}", fileUrl);
                return false;
            }
        }

        public bool IsValidFileType(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeInBytes)
        {
            return file.Length <= maxSizeInBytes;
        }
    }
}
