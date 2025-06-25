using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services.Auth
{
    public interface ITokenManager
    {
        Task<bool> IsTokenValidAsync(string token);
        Task SetTokenAsync(string token);
        Task ClearTokenAsync();
        Task<bool> RefreshTokenIfNeededAsync(string token);
        Task<string?> GetTokenAsync();
    }

    public class TokenManager : ITokenManager
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<TokenManager> _logger;

        private const string TOKEN_KEY = "accessToken";

        public TokenManager(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            ILogger<TokenManager> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _logger = logger;

            // 🔥 생성자에서 토큰 자동 설정
            _ = InitializeTokenAsync();
        }

        private async Task InitializeTokenAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("토큰 자동 설정 완료");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 자동 설정 중 오류 발생");
            }
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo.AddMinutes(-5) < DateTime.UtcNow)
                {
                    _logger.LogInformation("토큰 만료됨: ExpiryTime={ExpiryTime}", jwtToken.ValidTo);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 유효성 검사 중 오류 발생");
                return false;
            }
        }

        public async Task SetTokenAsync(string token)
        {
            try
            {
                await _localStorage.SetItemAsync(TOKEN_KEY, token);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogDebug("토큰 설정 완료");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 설정 중 오류 발생");
            }
        }

        public async Task ClearTokenAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync(TOKEN_KEY);
                _httpClient.DefaultRequestHeaders.Authorization = null;

                _logger.LogDebug("토큰 정리 완료");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 정리 중 오류 발생");
            }
        }

        public async Task<bool> RefreshTokenIfNeededAsync(string token)
        {
            try
            {
                if (await IsTokenValidAsync(token))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 갱신 확인 중 오류 발생");
                return false;
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>(TOKEN_KEY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 조회 중 오류 발생");
                return null;
            }
        }
    }
}
