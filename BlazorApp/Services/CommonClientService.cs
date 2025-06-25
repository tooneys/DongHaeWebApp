using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlazorApp.Models;
using BlazorApp.Services.Auth;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services.Common
{
    public interface ICommonClientService
    {
        Task<List<Optician>> GetOpticiansAsync();
        Task<Optician?> GetOpticianByIdAsync(string opticianId);
        Task<List<CodeDto>> GetCodeListAsync(string codeType);
        Task<List<RegionDto>> GetRegionsAsync();
    }

    public class CommonClientService : ICommonClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiResponseHandler _apiResponseHandler;
        private readonly ITokenManager _tokenManager;
        private readonly ILogger<CommonClientService> _logger;

        public CommonClientService(
            HttpClient httpClient,
            IApiResponseHandler apiResponseHandler,
            ITokenManager tokenManager,
            ILogger<CommonClientService> logger)
        {
            _httpClient = httpClient;
            _apiResponseHandler = apiResponseHandler;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        // 🔥 토큰 확인 및 설정 메서드
        private async Task EnsureAuthenticationAsync()
        {
            try
            {
                var token = await _tokenManager.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("토큰이 없습니다.");
                    throw new UnauthorizedAccessException("로그인이 필요합니다.");
                }

                if (!await _tokenManager.IsTokenValidAsync(token))
                {
                    _logger.LogWarning("토큰이 만료되었습니다.");
                    await _tokenManager.ClearTokenAsync();
                    throw new UnauthorizedAccessException("토큰이 만료되었습니다. 다시 로그인해주세요.");
                }

                // HttpClient에 토큰 설정 확인
                if (_httpClient.DefaultRequestHeaders.Authorization == null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("HttpClient에 토큰 설정 완료");
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw; // 인증 오류는 그대로 전파
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 확인 중 오류 발생");
                throw new UnauthorizedAccessException("인증 확인 중 오류가 발생했습니다.");
            }
        }

        public async Task<List<Optician>> GetOpticiansAsync()
        {
            try
            {
                _logger.LogInformation("안경사 목록 조회 시작");

                // 🔥 토큰 확인 및 설정
                await EnsureAuthenticationAsync();

                var response = await _httpClient.GetAsync("api/common/opticians");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<Optician>>(response);
                    var result = apiResponse?.Data ?? new List<Optician>();

                    _logger.LogInformation("안경사 목록 조회 완료: Count={Count}", result.Count);
                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _logger.LogWarning("안경사 목록 조회 실패: StatusCode={StatusCode}, Error={Error}",
                    response.StatusCode, errorMessage);

                throw new HttpRequestException($"안경사 목록 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 목록 조회 중 오류 발생");
                throw;
            }
        }

        public async Task<Optician?> GetOpticianByIdAsync(string opticianId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opticianId))
                    throw new ArgumentException("안경사 ID가 필요합니다.", nameof(opticianId));

                // 🔥 토큰 확인 및 설정
                await EnsureAuthenticationAsync();

                _logger.LogInformation("안경사 조회 시작: OpticianId={OpticianId}", opticianId);

                var response = await _httpClient.GetAsync($"api/common/opticians/{opticianId}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<Optician>(response);

                    _logger.LogInformation("안경사 조회 완료: OpticianId={OpticianId}, Found={Found}",
                        opticianId, apiResponse?.Data != null);

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("안경사를 찾을 수 없음: OpticianId={OpticianId}", opticianId);
                    return null;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"안경사 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<List<CodeDto>> GetCodeListAsync(string codeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codeType))
                    throw new ArgumentException("코드 타입이 필요합니다.", nameof(codeType));

                _logger.LogInformation("코드 목록 조회 시작: CodeType={CodeType}", codeType);

                var response = await _httpClient.GetAsync($"api/common/codes/{codeType}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<CodeDto>>(response);
                    var result = apiResponse?.Data ?? new List<CodeDto>();

                    _logger.LogInformation("코드 목록 조회 완료: CodeType={CodeType}, Count={Count}",
                        codeType, result.Count);

                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"코드 목록 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "코드 목록 조회 중 오류 발생: CodeType={CodeType}", codeType);
                throw;
            }
        }

        public async Task<List<RegionDto>> GetRegionsAsync()
        {
            try
            {
                _logger.LogInformation("지역 목록 조회 시작");

                var response = await _httpClient.GetAsync("api/common/regions");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<RegionDto>>(response);
                    var result = apiResponse?.Data ?? new List<RegionDto>();

                    _logger.LogInformation("지역 목록 조회 완료: Count={Count}", result.Count);
                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"지역 목록 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "지역 목록 조회 중 오류 발생");
                throw;
            }
        }
    }
}
