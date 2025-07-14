namespace WebApi.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string? relativePath);
        string GetFullUrl(string? relativePath);
        bool IsValidFileType(IFormFile file, string[] allowedExtensions);
        bool IsValidFileSize(IFormFile file, long maxSizeInBytes);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileUploadService> _logger;

        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };
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

        /// <summary>
        /// 파일을 업로드하고 상대 경로를 반환합니다.
        /// </summary>
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

                // 업로드 기본 경로 설정
                var uploadBasePath = GetUploadBasePath();
                var uploadsFolder = Path.Combine(uploadBasePath, "uploads", folder);

                // 폴더 생성
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

                // 상대 경로 반환
                var relativePath = $"/uploads/{folder}/{fileName}";

                _logger.LogInformation("파일 업로드 성공: {FileName} -> {RelativePath}", file.FileName, relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파일 업로드 실패: {FileName}", file.FileName);
                throw;
            }
        }

        /// <summary>
        /// 상대 경로를 받아 파일을 안전하게 삭제합니다.
        /// </summary>
        /// <param name="relativePath">삭제할 파일의 상대 경로 (예: /uploads/covers/filename.jpg)</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteFileAsync(string? relativePath)
        {
            // 1. 입력값 검증
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                _logger.LogDebug("삭제할 파일 경로가 비어있습니다.");
                return true; // 빈 경로는 성공으로 처리
            }

            try
            {
                // 2. 상대 경로 정규화
                var normalizedPath = NormalizeRelativePath(relativePath);
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
                    _logger.LogInformation("파일 삭제 성공: {RelativePath} -> {PhysicalPath}", relativePath, physicalPath);

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
                _logger.LogError(uaEx, "파일 삭제 권한 없음: {RelativePath}", relativePath);
                return false;
            }
            catch (DirectoryNotFoundException dnfEx)
            {
                _logger.LogError(dnfEx, "디렉토리를 찾을 수 없음: {RelativePath}", relativePath);
                return false;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "파일 I/O 오류: {RelativePath}", relativePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파일 삭제 중 예상치 못한 오류: {RelativePath}", relativePath);
                return false;
            }
        }

        /// <summary>
        /// 상대 경로를 전체 URL로 변환합니다.
        /// </summary>
        public string GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            var baseUrl = GetBaseUrl();
            var normalizedPath = NormalizeRelativePath(relativePath);

            return $"{baseUrl.TrimEnd('/')}{normalizedPath}";
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

        #region Private Helper Methods

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
            if (!string.IsNullOrEmpty(_environment.WebRootPath))
            {
                return _environment.WebRootPath;
            }

            // 3. ContentRootPath + wwwroot 사용
            var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
                _logger.LogInformation("wwwroot 폴더 생성: {Path}", wwwrootPath);
            }

            _logger.LogWarning("WebRootPath가 null입니다. 대체 경로 사용: {Path}", wwwrootPath);
            return wwwrootPath;
        }

        /// <summary>
        /// BaseURL을 가져옵니다.
        /// </summary>
        private string GetBaseUrl()
        {
            var baseUrl = _configuration["BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = _environment.IsDevelopment()
                    ? "https://localhost:5000"
                    : "https://api.donghaelens.com";

                _logger.LogWarning("BaseUrl 설정이 없어 기본값 사용: {BaseUrl}", baseUrl);
            }

            return baseUrl;
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

        #endregion
    }
}
