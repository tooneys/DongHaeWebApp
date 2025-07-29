using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace WebApi.Services.Common;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageService> _logger;
    private const int MaxWidth = 800;
    private const int Quality = 80;

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger, IConfiguration configuration)
    {
        _env = env;
        _logger = logger;
        _configuration = configuration;
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

    public async Task<(string url, byte[] resizedImage)> ProcessImageAsync(IFormFile file, string subFolder)
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
            string uploadFolder = Path.Combine(webRootPath, "uploads", subFolder);

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

            var imageUrl = $"/uploads/{subFolder}/{fileName}";

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

    public async Task<bool> DeleteImageAsync(string? path)
    {
        // 1. 입력값 검증
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogDebug("삭제할 파일 경로가 비어있습니다.");
            return true; // 빈 경로는 성공으로 처리
        }

        try
        {
            // 2. 상대 경로 정규화
            var normalizedPath = NormalizeRelativePath(path);
            _logger.LogDebug("정규화된 경로: {NormalizedPath}", normalizedPath);

            // 3. 물리적 경로 생성
            var physicalPath = GetPhysicalPathFromRelativePath(normalizedPath);
            _logger.LogDebug("물리적 경로: {PhysicalPath}", physicalPath);

            // 4. 보안 검증
            if (!IsSecureUploadPath(physicalPath))
            {
                _logger.LogWarning("보안 위험: 허용되지 않은 경로에서 파일 삭제 시도 - {PhysicalPath}", physicalPath);
                return false;
            }

            // 5. 파일 존재 확인 및 삭제
            if (File.Exists(physicalPath))
            {
                // 파일 속성 확인 (읽기 전용 등)
                var fileInfo = new FileInfo(physicalPath);
                if (fileInfo.IsReadOnly)
                {
                    _logger.LogWarning("읽기 전용 파일 삭제 시도: {PhysicalPath}", physicalPath);
                    return false;
                }

                File.Delete(physicalPath);
                _logger.LogInformation("파일 삭제 성공: {RelativePath} -> {PhysicalPath}", path, physicalPath);

                // 6. 빈 폴더 정리 (선택사항)
                await CleanupEmptyDirectoriesAsync(Path.GetDirectoryName(physicalPath));

                return true;
            }
            else
            {
                _logger.LogWarning("삭제할 파일이 존재하지 않습니다: {PhysicalPath}", physicalPath);
                return false; // 파일이 없으면 실패로 처리
            }
        }
        catch (UnauthorizedAccessException uaEx)
        {
            _logger.LogError(uaEx, "파일 삭제 권한 없음: {RelativePath}", path);
            return false;
        }
        catch (DirectoryNotFoundException dnfEx)
        {
            _logger.LogError(dnfEx, "디렉토리를 찾을 수 없음: {RelativePath}", path);
            return false;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "파일 I/O 오류: {RelativePath}", path);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 삭제 중 예상치 못한 오류: {RelativePath}", path);
            return false;
        }
    }

    /// <summary>
    /// 상대 경로를 정규화합니다.
    /// </summary>
    private string NormalizeRelativePath(string relativePath)
    {
        // URL에서 상대 경로 추출
        if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
        {
            var uri = new Uri(relativePath);
            relativePath = uri.AbsolutePath;
        }

        // 경로 정규화
        relativePath = relativePath.Replace('\\', '/');

        // 시작 슬래시 확인
        if (!relativePath.StartsWith("/"))
        {
            relativePath = "/" + relativePath;
        }

        // uploads로 시작하는지 확인
        if (!relativePath.StartsWith("/uploads/"))
        {
            throw new ArgumentException($"잘못된 파일 경로입니다: {relativePath}");
        }

        return relativePath;
    }

    /// <summary>
    /// 상대 경로를 물리적 경로로 변환합니다.
    /// </summary>
    private string GetPhysicalPathFromRelativePath(string relativePath)
    {
        var uploadBasePath = GetUploadBasePath();

        // 상대 경로에서 leading slash 제거
        var cleanPath = relativePath.TrimStart('/');

        // 경로 구분자 정규화
        cleanPath = cleanPath.Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(uploadBasePath, cleanPath);
    }

    /// <summary>
    /// 업로드 기본 경로를 가져옵니다.
    /// </summary>
    private string GetUploadBasePath()
    {
        // 1. 설정 파일에서 경로 확인
        var configPath = _configuration["FileUpload:BasePath"];
        if (!string.IsNullOrEmpty(configPath) && Directory.Exists(configPath))
        {
            return configPath;
        }

        // 2. WebRootPath 확인
        if (!string.IsNullOrEmpty(_env.WebRootPath))
        {
            return _env.WebRootPath;
        }

        // 3. ContentRootPath + wwwroot 사용
        var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
            _logger.LogInformation("wwwroot 폴더 생성: {Path}", wwwrootPath);
        }

        _logger.LogWarning("WebRootPath가 null입니다. 대체 경로 사용: {Path}", wwwrootPath);
        return wwwrootPath;
    }

    /// <summary>
    /// 경로가 안전한 업로드 경로인지 확인합니다.
    /// </summary>
    private bool IsSecureUploadPath(string physicalPath)
    {
        try
        {
            var uploadBasePath = GetUploadBasePath();
            var uploadsPath = Path.Combine(uploadBasePath, "uploads");

            // 절대 경로로 변환하여 비교
            var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
            var normalizedUploadsPath = Path.GetFullPath(uploadsPath);

            // uploads 폴더 내부인지 확인
            var isInUploadsFolder = normalizedPhysicalPath.StartsWith(normalizedUploadsPath, StringComparison.OrdinalIgnoreCase);

            // 경로 조작 공격 방지 (../ 등)
            var hasPathTraversal = normalizedPhysicalPath.Contains("..");

            var isSecure = isInUploadsFolder && !hasPathTraversal;

            if (!isSecure)
            {
                _logger.LogWarning("보안 검증 실패 - Physical: {PhysicalPath}, Uploads: {UploadsPath}, InFolder: {InFolder}, HasTraversal: {HasTraversal}",
                    normalizedPhysicalPath, normalizedUploadsPath, isInUploadsFolder, hasPathTraversal);
            }

            return isSecure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "경로 보안 검증 중 오류: {PhysicalPath}", physicalPath);
            return false;
        }
    }

    /// <summary>
    /// 빈 디렉토리를 정리합니다.
    /// </summary>
    private async Task CleanupEmptyDirectoriesAsync(string? directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return;

        try
        {
            await Task.Run(() =>
            {
                var directory = new DirectoryInfo(directoryPath);

                // uploads 폴더 자체는 삭제하지 않음
                if (directory.Name.Equals("uploads", StringComparison.OrdinalIgnoreCase))
                    return;

                // 빈 폴더이고 uploads 하위 폴더인 경우에만 삭제
                if (!directory.GetFiles().Any() && !directory.GetDirectories().Any())
                {
                    var uploadsPath = Path.Combine(GetUploadBasePath(), "uploads");
                    if (directoryPath.StartsWith(uploadsPath, StringComparison.OrdinalIgnoreCase))
                    {
                        directory.Delete();
                        _logger.LogInformation("빈 폴더 삭제: {DirectoryPath}", directoryPath);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "빈 폴더 정리 중 오류: {DirectoryPath}", directoryPath);
        }
    }
}