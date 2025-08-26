using BlazorApp.Models;
using BlazorApp.Services.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Services.PartnerCard
{
    public interface IPartnerCardClientService
    {
        Task<PartnerCardDto?> GetPartnerCardAsync(string opticianId);
        Task<List<OrderDto>> GetOrderDtosAsync(string opticianId, int year);
        Task<List<OpticianHistoryDto>> GetVisitHistoryAsync(string opticianId, DateTime? startDate = null, DateTime? endDate = null, string? searchTerm = null);
        Task<OpticianHistoryDto?> AddVisitHistoryAsync(OpticianHistoryDto historyDto);
        Task<OpticianHistoryDto?> GetVisitHistoryDetailAsync(int historyId);
        Task<OpticianHistoryDto?> UpdateVisitHistoryAsync(int historyId, OpticianHistoryDto historyDto);
        Task<bool> DeleteVisitHistoryAsync(int historyId);
        // 판촉물 관련 메서드 추가
        Task<OpticianPromotion?> AddPromotionAsync(string opticianId, string regDate, string promotion, IBrowserFile imageFile);
        Task<string> StorImageUploadAsync(IBrowserFile imageFile, int imageSlot, string opticianId);
        Task<bool> StoreImageDeleteAsync(int imageSlot, string opticianId);
    }

    public class PartnerCardClientService : IPartnerCardClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiResponseHandler _apiResponseHandler;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PartnerCardClientService> _logger;

        public PartnerCardClientService(
            HttpClient httpClient,
            IApiResponseHandler apiResponseHandler,
            INotificationService notificationService,
            ILogger<PartnerCardClientService> logger)
        {
            _httpClient = httpClient;
            _apiResponseHandler = apiResponseHandler;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<PartnerCardDto?> GetPartnerCardAsync(string opticianId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opticianId))
                    throw new ArgumentException("안경사 ID가 필요합니다.", nameof(opticianId));

                _logger.LogInformation("파트너카드 조회 시작: OpticianId={OpticianId}", opticianId);

                var response = await _httpClient.GetAsync($"api/partnercard/{opticianId}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<PartnerCardDto>(response);

                    _logger.LogInformation("파트너카드 조회 완료: OpticianId={OpticianId}, Found={Found}",
                        opticianId, apiResponse?.Data != null);

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("파트너카드를 찾을 수 없음: OpticianId={OpticianId}", opticianId);
                    return null;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"파트너카드 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파트너카드 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<List<OpticianHistoryDto>> GetVisitHistoryAsync(
            string opticianId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opticianId))
                    throw new ArgumentException("안경사 ID가 필요합니다.", nameof(opticianId));

                _logger.LogInformation("방문이력 목록 조회 시작: OpticianId={OpticianId}, StartDate={StartDate}, EndDate={EndDate}, SearchTerm={SearchTerm}",
                    opticianId, startDate, endDate, searchTerm);

                var queryParams = new List<string>();
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var endpoint = $"api/partnercard/visit-history/{opticianId}{queryString}";

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<OpticianHistoryDto>>(response);
                    var result = apiResponse?.Data ?? new List<OpticianHistoryDto>();

                    _logger.LogInformation("방문이력 목록 조회 완료: OpticianId={OpticianId}, Count={Count}",
                        opticianId, result.Count);

                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"방문이력 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<OpticianHistoryDto?> AddVisitHistoryAsync(OpticianHistoryDto historyDto)
        {
            try
            {
                if (historyDto == null)
                    throw new ArgumentNullException(nameof(historyDto));

                // 입력 검증
                ValidateVisitHistory(historyDto);

                _logger.LogInformation("방문이력 추가 시작: OpticianId={OpticianId}", historyDto.CD_CUST);

                var response = await _httpClient.PostAsJsonAsync("api/partnercard/visit-history", historyDto);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<OpticianHistoryDto>(response);

                    _logger.LogInformation("방문이력 추가 완료: OpticianId={OpticianId}, Id={Id}",
                        historyDto.CD_CUST, apiResponse?.Data?.ID);

                    return apiResponse?.Data;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"방문이력 추가 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 추가 중 오류 발생: OpticianId={OpticianId}", historyDto?.CD_CUST);
                throw;
            }
        }

        public async Task<OpticianHistoryDto?> GetVisitHistoryDetailAsync(int historyId)
        {
            try
            {
                if (historyId <= 0)
                    throw new ArgumentException("유효한 방문이력 ID가 필요합니다.", nameof(historyId));

                _logger.LogInformation("방문이력 상세 조회 시작: HistoryId={HistoryId}", historyId);

                var response = await _httpClient.GetAsync($"api/partnercard/visit-history/detail/{historyId}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<OpticianHistoryDto>(response);

                    _logger.LogInformation("방문이력 상세 조회 완료: HistoryId={HistoryId}, Found={Found}",
                        historyId, apiResponse?.Data != null);

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("방문이력을 찾을 수 없음: HistoryId={HistoryId}", historyId);
                    return null;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"방문이력 상세 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 상세 조회 중 오류 발생: HistoryId={HistoryId}", historyId);
                throw;
            }
        }

        public async Task<OpticianHistoryDto?> UpdateVisitHistoryAsync(int historyId, OpticianHistoryDto historyDto)
        {
            try
            {
                if (historyId <= 0)
                    throw new ArgumentException("유효한 방문이력 ID가 필요합니다.", nameof(historyId));

                if (historyDto == null)
                    throw new ArgumentNullException(nameof(historyDto));

                // 입력 검증
                ValidateVisitHistory(historyDto);

                _logger.LogInformation("방문이력 수정 시작: HistoryId={HistoryId}", historyId);

                historyDto.ID = historyId; // ID 설정
                var response = await _httpClient.PutAsJsonAsync($"api/partnercard/visit-history/{historyId}", historyDto);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<OpticianHistoryDto>(response);

                    _logger.LogInformation("방문이력 수정 완료: HistoryId={HistoryId}", historyId);
                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("수정할 방문이력을 찾을 수 없음: HistoryId={HistoryId}", historyId);
                    return null;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"방문이력 수정 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 수정 중 오류 발생: HistoryId={HistoryId}", historyId);
                throw;
            }
        }

        public async Task<bool> DeleteVisitHistoryAsync(int historyId)
        {
            try
            {
                if (historyId <= 0)
                    throw new ArgumentException("유효한 방문이력 ID가 필요합니다.", nameof(historyId));

                _logger.LogInformation("방문이력 삭제 시작: HistoryId={HistoryId}", historyId);

                var response = await _httpClient.DeleteAsync($"api/partnercard/visit-history/{historyId}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("방문이력 삭제 완료: HistoryId={HistoryId}", historyId);
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("삭제할 방문이력을 찾을 수 없음: HistoryId={HistoryId}", historyId);
                    return false;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"방문이력 삭제 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 삭제 중 오류 발생: HistoryId={HistoryId}", historyId);
                throw;
            }
        }

        public async Task<OpticianPromotion?> AddPromotionAsync(string opticianId, string regDate, string promotion, IBrowserFile imageFile)
        {
            try
            {
                // Multipart 요청 생성
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(regDate), "RegDate");
                content.Add(new StringContent(opticianId), "OpticianId");
                content.Add(new StringContent(promotion), "Promotion");

                // 파일 추가
                var fileContent = new StreamContent(imageFile.OpenReadStream(10 * 1024 * 1024));
                content.Add(fileContent, "ImageFile", imageFile.Name);

                // 서버 요청
                var response = await _httpClient.PostAsync("/api/partnercard/promotion", content);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<OpticianPromotion>(response);

                    _logger.LogInformation("판촉물 추가 완료: OpticianId={OpticianId}, PromotionId={PromotionId}",
                        opticianId, apiResponse?.Data?.Id);

                    return apiResponse?.Data;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"판촉물 추가 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "판촉물 추가 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        private void ValidateVisitHistory(OpticianHistoryDto historyDto)
        {
            if (string.IsNullOrWhiteSpace(historyDto.TX_REASON))
                throw new ArgumentException("방문사유는 필수입니다.", nameof(historyDto));

            if (string.IsNullOrWhiteSpace(historyDto.TX_PURPOSE))
                throw new ArgumentException("방문목적은 필수입니다.", nameof(historyDto));

            if (string.IsNullOrWhiteSpace(historyDto.DT_COMP))
                throw new ArgumentException("방문일자는 필수입니다.", nameof(historyDto));

            if (!DateTime.TryParse(historyDto.DT_COMP, out var visitDate))
                throw new ArgumentException("올바른 날짜 형식이 아닙니다.", nameof(historyDto));

            if (visitDate > DateTime.Today)
                throw new ArgumentException("방문일자는 오늘 이후로 설정할 수 없습니다.", nameof(historyDto));
        }

        public async Task<string> StorImageUploadAsync(IBrowserFile imageFile, int imageSlot, string opticianId)
        {
            try
            {
                if (imageFile == null)
                    throw new ArgumentNullException(nameof(imageFile));

                // Multipart 요청 생성
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(imageSlot.ToString()), "ImageSlot");
                content.Add(new StringContent(opticianId), "OpticianId");

                var fileContent = new StreamContent(imageFile.OpenReadStream(10 * 1024 * 1024));
                content.Add(fileContent, "ImageFile", imageFile.Name);

                // 서버 요청
                var response = await _httpClient.PostAsync("/api/partnercard/upload-image", content);
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<string>(response);
                    _logger.LogInformation("이미지 업로드 완료: FileName={FileName}", imageFile.Name);
                    _notificationService.ShowSuccess($"파일 '{imageFile.Name}'이 성공적으로 업로드되었습니다.");
                    return apiResponse?.Data ?? string.Empty;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _notificationService.ShowError($"파일 '{imageFile.Name}' 업로드 실패: {errorMessage}");
                throw new HttpRequestException($"이미지 업로드 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"파일 '{imageFile.Name}' 업로드 중 오류가 발생했습니다.");
                _logger.LogError(ex, "이미지 업로드 중 오류 발생: FileName={FileName}", imageFile.Name);
                throw;
            }
        }

        public async Task<bool> StoreImageDeleteAsync(int imageSlot, string opticianId)
        {
            try
            {
                // Multipart 요청 생성
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(imageSlot.ToString()), "ImageSlot");
                content.Add(new StringContent(opticianId), "OpticianId");
                
                // 서버 요청
                var response = await _httpClient.PostAsync("/api/partnercard/delete-image", content);
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<string>(response);
                    _logger.LogInformation($"이미지 삭제 완료: opticianId={opticianId}, imageSlot={imageSlot}");
                    _notificationService.ShowSuccess($"{imageSlot}번째 이미지가 삭제되었습니다.");
                    return true;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _notificationService.ShowError($"{imageSlot}번재 이미지 삭제가 실패하였습니다.");
                throw new HttpRequestException($"이미지 삭제 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"{imageSlot}번재 이미지 삭제 중 오류 발생.");
                _logger.LogError(ex, $"이미지 삭제 중 오류 발생: imageSlot={imageSlot}");
                throw;
            }
        }

        public async Task<List<OrderDto>> GetOrderDtosAsync(string opticianId, int year)
        {
            try
            {
                if (string.IsNullOrEmpty(opticianId))
                    throw new ArgumentException("안경원코드가 필요합니다.", nameof(opticianId));

                _logger.LogInformation($"매출정보 조회 시작: OpticianId={opticianId}");

                var response = await _httpClient.GetAsync($"api/partnercard/orders/{opticianId}?year={year}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<OrderDto>>(response);

                    _logger.LogInformation($"매출정보 조회 완료: OpticianId={opticianId}, Found={apiResponse?.Data != null}");

                    return apiResponse?.Data ?? new List<OrderDto>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"매출정보를 찾을 수 없음: OpticianId={opticianId}");
                    return new List<OrderDto>();
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"매출정보 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"매출정보 조회 중 오류 발생: OpticianId={opticianId}");
                throw;
            }
        }
    }
}
