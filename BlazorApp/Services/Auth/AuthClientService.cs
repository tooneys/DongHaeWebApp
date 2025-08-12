using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using BlazorApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services.Auth
{
    public class AuthClientService : IAuthClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ITokenManager _tokenManager;
        private readonly IApiResponseHandler _apiResponseHandler;
        private readonly ILogger<AuthClientService> _logger;

        private const string TOKEN_KEY = "accessToken";
        private const string USER_KEY = "userProfile";
        private const string SELECTEDEMPLOYEE_KEY = "selectedEmployeeCode";

        public AuthClientService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            ITokenManager tokenManager,
            IApiResponseHandler apiResponseHandler,
            ILogger<AuthClientService> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _tokenManager = tokenManager;
            _apiResponseHandler = apiResponseHandler;
            _logger = logger;
        }

        public async Task<FormResult> LoginAsync(string userId, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                {
                    return new FormResult
                    {
                        Successed = false,
                        Errors = ["사용자 ID와 비밀번호를 입력해주세요."]
                    };
                }

                _logger.LogInformation("로그인 시도: UserId={UserId}", userId);

                var loginRequest = new { UserId = userId, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<UserProfile>(response);

                    if (apiResponse?.Success == true && !string.IsNullOrEmpty(apiResponse.Data?.Token))
                    {
                        await StoreAuthenticationDataAsync(apiResponse.Data);
                        await _tokenManager.SetTokenAsync(apiResponse.Data.Token);

                        _logger.LogInformation("로그인 성공: UserId={UserId}", userId);
                        return new FormResult { Successed = true };
                    }
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _logger.LogWarning("로그인 실패: UserId={UserId}, StatusCode={StatusCode}, Error={Error}",
                    userId, response.StatusCode, errorMessage);

                return new FormResult
                {
                    Successed = false,
                    Errors = [errorMessage ?? "아이디 또는 비밀번호가 잘못되었습니다."]
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "로그인 중 네트워크 오류 발생: UserId={UserId}", userId);
                return new FormResult
                {
                    Successed = false,
                    Errors = ["서버 연결 오류가 발생했습니다."]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그인 중 예상치 못한 오류 발생: UserId={UserId}", userId);
                return new FormResult
                {
                    Successed = false,
                    Errors = ["로그인 처리 중 오류가 발생했습니다."]
                };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                _logger.LogInformation("로그아웃 시작");

                await ClearAuthenticationDataAsync();
                await _tokenManager.ClearTokenAsync();

                _logger.LogInformation("로그아웃 완료");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그아웃 중 오류 발생");
            }
        }

        public async Task<UserProfile?> GetCurrentUserAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<UserProfile>(USER_KEY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "현재 사용자 정보 조회 중 오류 발생");
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
                if (string.IsNullOrEmpty(token))
                    return false;

                return await _tokenManager.IsTokenValidAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "인증 상태 확인 중 오류 발생");
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await _tokenManager.IsTokenValidAsync(token);
        }

        public async Task<bool> RefreshTokenIfNeededAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
                if (string.IsNullOrEmpty(token))
                    return false;

                return await _tokenManager.RefreshTokenIfNeededAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 갱신 중 오류 발생");
                return false;
            }
        }

        private async Task StoreAuthenticationDataAsync(UserProfile userProfile)
        {
            await _localStorage.SetItemAsync(TOKEN_KEY, userProfile.Token);
            await _localStorage.SetItemAsync(USER_KEY, userProfile);
        }

        private async Task ClearAuthenticationDataAsync()
        {
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(USER_KEY);
            await _localStorage.RemoveItemAsync(SELECTEDEMPLOYEE_KEY);
        }
    }
}
