using Blazored.LocalStorage;

namespace BlazorApp.Services
{
    public interface ICacheService
    {
        Task InitializeAsync();
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task ClearAsync();
    }

    public class LocalStorageCacheService : ICacheService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<LocalStorageCacheService> _logger;
        private const string CACHE_PREFIX = "cache_";

        public LocalStorageCacheService(ILocalStorageService localStorage, ILogger<LocalStorageCacheService> logger)
        {
            _localStorage = localStorage;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("캐시 서비스 초기화 시작");
                // 만료된 캐시 정리
                await CleanExpiredCacheAsync();
                _logger.LogInformation("캐시 서비스 초기화 완료");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "캐시 서비스 초기화 중 오류 발생");
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cacheKey = $"{CACHE_PREFIX}{key}";
                var cacheItem = await _localStorage.GetItemAsync<CacheItem<T>>(cacheKey);

                if (cacheItem != null && cacheItem.ExpiryTime > DateTime.UtcNow)
                {
                    return cacheItem.Value;
                }

                if (cacheItem != null)
                {
                    await _localStorage.RemoveItemAsync(cacheKey);
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "캐시 조회 중 오류 발생: Key={Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var cacheKey = $"{CACHE_PREFIX}{key}";
                var expiryTime = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(30));

                var cacheItem = new CacheItem<T>
                {
                    Value = value,
                    ExpiryTime = expiryTime
                };

                await _localStorage.SetItemAsync(cacheKey, cacheItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "캐시 저장 중 오류 발생: Key={Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var cacheKey = $"{CACHE_PREFIX}{key}";
                await _localStorage.RemoveItemAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "캐시 삭제 중 오류 발생: Key={Key}", key);
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await _localStorage.ClearAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "캐시 전체 삭제 중 오류 발생");
            }
        }

        private async Task CleanExpiredCacheAsync()
        {
            // 만료된 캐시 항목 정리 로직
        }
    }

    public class CacheItem<T>
    {
        public T Value { get; set; } = default!;
        public DateTime ExpiryTime { get; set; }
    }
}
