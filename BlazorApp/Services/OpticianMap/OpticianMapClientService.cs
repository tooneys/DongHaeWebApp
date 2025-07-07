using System.Net;
using BlazorApp.Models;
using BlazorApp.Services.Auth;
using System.Net.Http.Json;

namespace BlazorApp.Services.OpticianMap
{
    public interface IOpticianMapClientService
    {
        Task<List<OpticianMapDto>> GetOpticianMapAllAsync();
        Task<List<UnRegMarkerHistory>> GetVisitHistoryById(OpticianMapDto markerDto);
        Task<UnRegMarkerHistory?> AddVisitHistoryAsync(UnRegMarkerHistory historyDto);
        Task<List<OpticianMapDto>> GetOpticianMapByRegionAsync(string region);
        Task<OpticianMapDto?> GetOpticianByIdAsync(int id);
        Task<List<MarkerData>> GetNearbyOpticiansAsync(double latitude, double longitude, double radiusKm);
        Task<bool> UpdateOpticianLocationAsync(int id, decimal geoX, decimal geoY);
        Task LogRegistrationRequestAsync(int opticianId, string opticianName);
        Task<List<MarkerData>> ConvertToMarkersAsync(List<OpticianMapDto> opticians, LocationData? userLocation = null);
        Task UpdateOpticianManage(OpticianMapDto Marker);
    }

    public class OpticianMapClientService : IOpticianMapClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiResponseHandler _apiResponseHandler;
        private readonly ILogger<OpticianMapClientService> _logger;

        public OpticianMapClientService(
            HttpClient httpClient,
            IApiResponseHandler apiResponseHandler,
            ILogger<OpticianMapClientService> logger)
        {
            _httpClient = httpClient;
            _apiResponseHandler = apiResponseHandler;
            _logger = logger;
        }

        public async Task<List<OpticianMapDto>> GetOpticianMapAllAsync()
        {
            try
            {
                _logger.LogInformation("전체 안경사 지도 데이터 조회 시작");

                var response = await _httpClient.GetAsync("api/opticianmap");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<OpticianMapDto>>(response);
                    var result = apiResponse?.Data ?? new List<OpticianMapDto>();

                    _logger.LogInformation("전체 안경사 지도 데이터 조회 완료: Count={Count}", result.Count);
                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _logger.LogWarning("안경사 지도 데이터 조회 실패: StatusCode={StatusCode}, Error={Error}",
                    response.StatusCode, errorMessage);

                throw new HttpRequestException($"안경사 지도 데이터 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 지도 데이터 조회 중 오류 발생");
                throw;
            }
        }
        
        public async Task<List<UnRegMarkerHistory>> GetVisitHistoryById(OpticianMapDto markerDto)
        {
            try
            {
                _logger.LogInformation("미등록 안경원 방문이력 조회 시작");

                var response = await _httpClient.GetAsync($"api/opticianmap/visit-history?OpnSfTeamCode={markerDto.OpnSfTeamCode}&MgtNo={markerDto.MgtNo}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<UnRegMarkerHistory>>(response);
                    var result = apiResponse?.Data ?? new List<UnRegMarkerHistory>();

                    _logger.LogInformation("미등록 안경원 방문이력 조회 완료: Count={Count}", result.Count);
                    return result;
                }
                
                // 404 Not Found인 경우 빈 리스트 반환
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("미등록 안경원 방문이력이 존재하지 않음: OpnSfTeamCode={OpnSfTeamCode}, MgtNo={MgtNo}", 
                        markerDto.OpnSfTeamCode, markerDto.MgtNo);
                    return new List<UnRegMarkerHistory>();
                }
                
                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                _logger.LogWarning("미등록 안경원 방문이력 조회 실패: StatusCode={StatusCode}, Error={Error}",
                    response.StatusCode, errorMessage);

                throw new HttpRequestException($"미등록 안경원 방문이력 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "미등록 안경원 방문이력 조회 중 오류 발생");
                throw;
            }
        }
        
        public async Task<UnRegMarkerHistory?> AddVisitHistoryAsync(UnRegMarkerHistory historyDto)
        {
            try
            {
                if (historyDto == null)
                    throw new ArgumentNullException(nameof(historyDto));

                _logger.LogInformation("미등록 안경원 방문이력 추가 시작: MgtNo={MgtNo}", historyDto.MgtNo);

                var response = await _httpClient.PostAsJsonAsync("api/opticianmap/visit-history", historyDto);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<UnRegMarkerHistory>(response);

                    _logger.LogInformation("미등록 안경원 방문이력 추가 완료: MgtNo={MgtNo}, Seq={Seq}",
                        historyDto.MgtNo, apiResponse?.Data?.Seq);

                    return apiResponse?.Data;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"미등록 안경원 방문이력 추가 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "미등록 안경원 방문이력 추가 중 오류 발생: MgtNo={MgtNo}", historyDto?.MgtNo);
                throw;
            }
        }
        
        public async Task<List<OpticianMapDto>> GetOpticianMapByRegionAsync(string region)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(region))
                    throw new ArgumentException("지역명이 필요합니다.", nameof(region));

                _logger.LogInformation("지역별 안경사 지도 데이터 조회 시작: Region={Region}", region);

                var response = await _httpClient.GetAsync($"api/opticianmap/region/{region}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<OpticianMapDto>>(response);
                    var result = apiResponse?.Data ?? new List<OpticianMapDto>();

                    _logger.LogInformation("지역별 안경사 지도 데이터 조회 완료: Region={Region}, Count={Count}",
                        region, result.Count);

                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"지역별 안경사 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "지역별 안경사 조회 중 오류 발생: Region={Region}", region);
                throw;
            }
        }

        public async Task<OpticianMapDto?> GetOpticianByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("유효한 안경사 ID가 필요합니다.", nameof(id));

                _logger.LogInformation("안경사 정보 조회 시작: Id={Id}", id);

                var response = await _httpClient.GetAsync($"api/opticianmap/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<OpticianMapDto>(response);

                    _logger.LogInformation("안경사 정보 조회 완료: Id={Id}, Found={Found}",
                        id, apiResponse?.Data != null);

                    return apiResponse?.Data;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("안경사 정보를 찾을 수 없음: Id={Id}", id);
                    return null;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"안경사 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 조회 중 오류 발생: Id={Id}", id);
                throw;
            }
        }

        public async Task<List<MarkerData>> GetNearbyOpticiansAsync(double latitude, double longitude, double radiusKm)
        {
            try
            {
                _logger.LogInformation("주변 안경사 조회 시작: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km",
                    latitude, longitude, radiusKm);

                var response = await _httpClient.GetAsync(
                    $"api/opticianmap/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<List<MarkerData>>(response);
                    var result = apiResponse?.Data ?? new List<MarkerData>();

                    _logger.LogInformation("주변 안경사 조회 완료: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km, Count={Count}",
                        latitude, longitude, radiusKm, result.Count);

                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"주변 안경사 조회 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "주변 안경사 조회 중 오류 발생: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km",
                    latitude, longitude, radiusKm);
                throw;
            }
        }

        public async Task<bool> UpdateOpticianLocationAsync(int id, decimal geoX, decimal geoY)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("유효한 안경사 ID가 필요합니다.", nameof(id));

                _logger.LogInformation("안경사 위치 정보 업데이트 시작: Id={Id}, GeoX={GeoX}, GeoY={GeoY}",
                    id, geoX, geoY);

                var updateRequest = new
                {
                    Id = id,
                    GeoX = geoX,
                    GeoY = geoY
                };

                var response = await _httpClient.PutAsJsonAsync($"api/opticianmap/{id}/location", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await _apiResponseHandler.HandleResponseAsync<bool>(response);
                    var result = apiResponse?.Data ?? false;

                    _logger.LogInformation("안경사 위치 정보 업데이트 완료: Id={Id}, Success={Success}",
                        id, result);

                    return result;
                }

                var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                throw new HttpRequestException($"위치 정보 업데이트 실패: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 위치 정보 업데이트 중 오류 발생: Id={Id}", id);
                throw;
            }
        }

        public async Task LogRegistrationRequestAsync(int opticianId, string opticianName)
        {
            try
            {
                _logger.LogInformation("등록 요청 로그 저장 시작: OpticianId={OpticianId}, OpticianName={OpticianName}",
                    opticianId, opticianName);

                var logRequest = new
                {
                    OpticianId = opticianId,
                    OpticianName = opticianName,
                    RequestTime = DateTime.UtcNow,
                    RequestType = "Registration"
                };

                var response = await _httpClient.PostAsJsonAsync("api/opticianmap/registration-request", logRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("등록 요청 로그 저장 완료: OpticianId={OpticianId}", opticianId);
                }
                else
                {
                    var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                    _logger.LogWarning("등록 요청 로그 저장 실패: OpticianId={OpticianId}, Error={Error}",
                        opticianId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "등록 요청 로그 저장 중 오류 발생: OpticianId={OpticianId}", opticianId);
                // 로그 저장 실패는 예외를 다시 던지지 않음 (비즈니스 로직에 영향 없음)
            }
        }

        public async Task<List<MarkerData>> ConvertToMarkersAsync(List<OpticianMapDto> opticians, LocationData? userLocation = null)
        {
            try
            {
                _logger.LogInformation("안경사 데이터를 마커 데이터로 변환 시작: Count={Count}", opticians.Count);

                var markers = new List<MarkerData>();

                foreach (var optician in opticians)
                {
                    var marker = new MarkerData
                    {
                        Id = optician.Id,
                        BplcNm = optician.BplcNm,
                        GeoX = (double)optician.GeoX,
                        GeoY = (double)optician.GeoY,
                        IsReg = optician.IsReg,
                        RegStatus = optician.IsReg ? "등록됨" : "미등록",
                        Latitude = (double)optician.GeoY, // GeoY를 Latitude로 매핑
                        Longitude = (double)optician.GeoX, // GeoX를 Longitude로 매핑
                        UtmkX = (double)optician.GeoX,
                        UtmkY = (double)optician.GeoY,
                        CustCode = optician.CustCode,
                        Distance = 0 // 기본값
                    };

                    // 사용자 위치가 있으면 거리 계산
                    if (userLocation != null)
                    {
                        marker.Distance = CalculateDistance(
                            userLocation.Lat, userLocation.Lng,
                            marker.Latitude, marker.Longitude);
                    }

                    markers.Add(marker);
                }

                // 거리순으로 정렬 (사용자 위치가 있는 경우)
                if (userLocation != null)
                {
                    markers = markers.OrderBy(m => m.Distance).ToList();
                }

                _logger.LogInformation("마커 데이터 변환 완료: MarkerCount={MarkerCount}", markers.Count);
                return markers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마커 데이터 변환 중 오류 발생");
                throw;
            }
        }

        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            try
            {
                const double R = 6371; // 지구 반지름 (km)

                var dLat = ToRadians(lat2 - lat1);
                var dLng = ToRadians(lng2 - lng1);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                return R * c; // 거리 (km)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "거리 계산 중 오류 발생: Lat1={Lat1}, Lng1={Lng1}, Lat2={Lat2}, Lng2={Lng2}",
                    lat1, lng1, lat2, lng2);
                return 0;
            }
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public async Task UpdateOpticianManage(OpticianMapDto Marker)
        {
            try
            {
                _logger.LogInformation($"안경원 상태변경 시작: MgtNo={Marker.MgtNo}, BplcNm={Marker.BplcNm}");

                var statusRequest = new
                {
                    OpnSfTeamCode = Marker.OpnSfTeamCode,
                    MgtNo = Marker.MgtNo,
                    OpticianManage = Marker.OpticianManage,
                };

                var response = await _httpClient.PostAsJsonAsync("api/opticianmap/status_change", statusRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"안경원 상태변경 완료: MgtNo={Marker.MgtNo}");
                }
                else
                {
                    var errorMessage = await _apiResponseHandler.ExtractErrorMessageAsync(response);
                    _logger.LogWarning($"안경원 상태변경 실패: MgtNo={Marker.MgtNo}, Error={errorMessage}");
                        
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"안경원 상태변경 중 오류 발생: MgtNo={Marker.MgtNo}");
                // 로그 저장 실패는 예외를 다시 던지지 않음 (비즈니스 로직에 영향 없음)
            }
        }
    }
}
