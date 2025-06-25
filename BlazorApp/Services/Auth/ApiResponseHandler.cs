using System.Text.Json;
using BlazorApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services.Auth
{
    public interface IApiResponseHandler
    {
        Task<ApiResponse<T>?> HandleResponseAsync<T>(HttpResponseMessage response);
        Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response);
    }

    public class ApiResponseHandler : IApiResponseHandler
    {
        private readonly ILogger<ApiResponseHandler> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiResponseHandler(ILogger<ApiResponseHandler> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null // PascalCase 유지
            };
        }

        public async Task<ApiResponse<T>?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("API 응답 내용이 비어있음: StatusCode={StatusCode}", response.StatusCode);
                    return null;
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);

                if (apiResponse == null)
                {
                    _logger.LogWarning("API 응답 역직렬화 실패: Content={Content}", content);
                }

                return apiResponse;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "API 응답 JSON 파싱 중 오류 발생");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API 응답 처리 중 오류 발생");
                return null;
            }
        }

        public async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                return errorResponse?.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "오류 메시지 추출 중 오류 발생");
                return null;
            }
        }
    }
}
