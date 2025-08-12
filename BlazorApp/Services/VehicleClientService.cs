using BlazorApp.Models;
using BlazorApp.Pages.Vehicle;
using BlazorApp.Services.Auth;
using Blazored.LocalStorage;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace BlazorApp.Services
{
    public interface IVehicleClientService
    {
        Task<List<Vehicle>> GetVehiclesById();
        Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int Seq);
    }

    public class VehicleClientService : IVehicleClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;
        private readonly IApiResponseHandler _apiResponseHandler;
        private readonly INotificationService _notificationService;
        private readonly ILogger<VehicleClientService> _logger;

        public VehicleClientService(
            HttpClient httpClient,
            ILocalStorageService localStorageService,
            IApiResponseHandler apiResponseHandler,
            INotificationService notificationService,
            ILogger<VehicleClientService> logger
        )
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
            _apiResponseHandler = apiResponseHandler;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<Vehicle>> GetVehiclesById()
        {
            try
            {
                string empCode = string.Empty;
                var userProfile = await _localStorageService.GetItemAsync<UserProfile>("userProfile");

                if (userProfile != null)
                {
                    empCode = userProfile.UserId;
                }

                _logger.LogInformation($"차량일지 내역 조회 시작: empCode={empCode}");

                var response = await _httpClient.GetAsync($"api/vehicle/{empCode}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<Vehicle>>(response);

                    _logger.LogInformation($"차량일지 내역 조회 완료: empCode={empCode}, Found={apiResponse?.Data != null}");

                    return apiResponse?.Data ?? new List<Vehicle>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"차량일지 내역을 찾을 수 없음: empCode={empCode}");
                    return new List<Vehicle>();
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"차량일지 내역 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"차량일지 내역 조회 중 오류 발생: {ex.Message}");
                throw;
            }
        }

        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
        {
            try
            {
                _logger.LogInformation("차량일지 작성 시작");

                ValidateVehicle(vehicle);

                var response = await _httpClient.PostAsJsonAsync("api/vehicle", vehicle);

                if (response.IsSuccessStatusCode) 
                { 
                    _logger.LogInformation("차량일지 작성 완료");
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<Vehicle>(response);
                    return apiResponse?.Data ?? throw new InvalidOperationException("차량일지 작성 후 반환된 데이터가 없습니다.");
                }
                else
                {
                    var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                    _logger.LogError($"차량일지 작성 실패: {errorMessage}");
                    throw new HttpRequestException($"차량일지 작성 실패: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차량일지 작성 중 오류 발생");
                throw;
            }
            
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                _logger.LogInformation("차량일지 수정 시작");

                ValidateVehicle(vehicle);

                var response = await _httpClient.PostAsJsonAsync("api/vehicle/update", vehicle);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("차량일지 수정 완료");
                    var result = await _apiResponseHandler.HandleResponseAsync<Vehicle>(response);
                    return result?.Data ?? throw new InvalidOperationException("차량일지 수정 후 반환된 데이터가 없습니다.");
                }
                else
                {
                    var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                    _logger.LogError($"차량일지 수정 실패: {errorMessage}");
                    throw new HttpRequestException($"차량일지 수정 실패: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차량일지 수정 중 오류 발생");
                throw;
            }
        }

        private void ValidateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "차량 정보가 null입니다.");
            }
            if (vehicle.DT_COMP == null)
            {
                _notificationService.ShowError("등록일자가 비어 있습니다.");
                throw new ArgumentException("등록일자가 비어 있습니다.", nameof(vehicle.DT_COMP));
            }
            if (string.IsNullOrWhiteSpace(vehicle.CD_EMP))
            {
                _notificationService.ShowError("사원 코드가 비어 있습니다.");
                throw new ArgumentException("사원 코드가 비어 있습니다.", nameof(vehicle.CD_EMP));
            }
            if (string.IsNullOrWhiteSpace(vehicle.NO_CAR))
            {
                _notificationService.ShowError("차량 번호가 비어 있습니다.");
                throw new ArgumentException("차량 번호가 비어 있습니다.", nameof(vehicle.NO_CAR));
            }
        }

        public async Task DeleteVehicleAsync(int Seq)
        {
            try
            {
                _logger.LogInformation("차량일지 삭제 시작");

                var response = await _httpClient.PostAsJsonAsync("api/vehicle/delete", Seq);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("차량일지 삭제 완료");
                    await _apiResponseHandler.HandleResponseAsync<string>(response);
                }
                else
                {
                    var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                    _logger.LogError($"차량일지 삭제 실패: {errorMessage}");
                    throw new HttpRequestException($"차량일지 삭제 실패: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차량일지 삭제 중 오류 발생");
                throw;
            }
        }
    }
}