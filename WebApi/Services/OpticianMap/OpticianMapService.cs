using Dapper;
using System.Data;
using WebApi.Models;
using WebApi.Infrastructure;

namespace WebApi.Services.OpticianMap
{
    public class OpticianMapService : IOpticianMapService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<OpticianMapService> _logger;

        public OpticianMapService(IDbConnectionFactory connectionFactory, ILogger<OpticianMapService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<OpticianGeoLocation>> GetOpticianMapAll()
        {
            try
            {
                _logger.LogInformation("안경사 지도 데이터 조회 시작");

                using var connection = _connectionFactory.CreateConnection();

                var partner = await connection.QueryAsync<OpticianGeoLocation>(
                    "GetOpticianMapAll",
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("안경사 지도 데이터 조회 완료: Count={Count}", partner?.Count() ?? 0);
                return partner ?? Enumerable.Empty<OpticianGeoLocation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 지도 데이터 조회 중 오류 발생");
                throw;
            }
        }

        public async Task<IEnumerable<OpticianGeoLocation>> GetOpticianMapByRegion(string region)
        {
            try
            {
                _logger.LogInformation("지역별 안경사 지도 데이터 조회 시작: Region={Region}", region);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Region", region);

                var partner = await connection.QueryAsync<OpticianGeoLocation>(
                    "GetOpticianMapByRegion",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("지역별 안경사 지도 데이터 조회 완료: Region={Region}, Count={Count}", region, partner?.Count() ?? 0);
                return partner ?? Enumerable.Empty<OpticianGeoLocation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "지역별 안경사 지도 데이터 조회 중 오류 발생: Region={Region}", region);
                throw;
            }
        }

        public async Task<OpticianGeoLocation> GetOpticianLocationById(string opticianId)
        {
            try
            {
                _logger.LogInformation("안경사 위치 정보 조회 시작: OpticianId={OpticianId}", opticianId);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);

                var location = await connection.QueryFirstOrDefaultAsync<OpticianGeoLocation>(
                    "GetOpticianLocationById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("안경사 위치 정보 조회 완료: OpticianId={OpticianId}, Found={Found}", opticianId, location != null);
                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 위치 정보 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<OpticianGeoLocation>> GetNearbyOpticians(double latitude, double longitude, double radiusKm)
        {
            try
            {
                _logger.LogInformation("주변 안경사 조회 시작: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km",
                    latitude, longitude, radiusKm);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Latitude", latitude);
                parameters.Add("@Longitude", longitude);
                parameters.Add("@RadiusKm", radiusKm);

                var nearbyOpticians = await connection.QueryAsync<OpticianGeoLocation>(
                    "GetNearbyOpticians",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("주변 안경사 조회 완료: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km, Count={Count}",
                    latitude, longitude, radiusKm, nearbyOpticians?.Count() ?? 0);

                return nearbyOpticians ?? Enumerable.Empty<OpticianGeoLocation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "주변 안경사 조회 중 오류 발생: Lat={Latitude}, Lng={Longitude}, Radius={RadiusKm}km",
                    latitude, longitude, radiusKm);
                throw;
            }
        }

        public async Task<bool> UpdateOpticianLocation(string opticianId, double latitude, double longitude)
        {
            try
            {
                _logger.LogInformation("안경사 위치 정보 업데이트 시작: OpticianId={OpticianId}, Lat={Latitude}, Lng={Longitude}",
                    opticianId, latitude, longitude);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);
                parameters.Add("@Latitude", latitude);
                parameters.Add("@Longitude", longitude);

                var affectedRows = await connection.ExecuteAsync(
                    "UpdateOpticianLocation",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var isUpdated = affectedRows > 0;
                _logger.LogInformation("안경사 위치 정보 업데이트 완료: OpticianId={OpticianId}, Success={Success}",
                    opticianId, isUpdated);

                return isUpdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 위치 정보 업데이트 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<UnRegMarkerHistory> AddVisitHistoryAsync(UnRegMarkerHistory historyDto)
        {
            try
            {
                _logger.LogInformation("방문이력 추가 시작: OpticianId={OpticianId}", historyDto.MgtNo);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@OpnSfTeamCode", historyDto.OpnSfTeamCode);
                parameters.Add("@MgtNo", historyDto.MgtNo);
                parameters.Add("@DT_COMP", historyDto.DT_COMP.Replace("-", ""));
                parameters.Add("@TX_REASON", historyDto.TX_REASON);
                parameters.Add("@TX_PURPOSE", historyDto.TX_PURPOSE);
                parameters.Add("@TX_NOTE", historyDto.TX_NOTE);
                parameters.Add("@Seq", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "AddUnRegOpticianHistory", // 방문이력 추가 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                historyDto.Seq = parameters.Get<int>("@Seq");

                _logger.LogInformation("방문이력 추가 완료: OpnSfTeamCode={OpnSfTeamCode}, MgtNo={MgtNo}, Seq={Seq}", historyDto.OpnSfTeamCode, historyDto.MgtNo, historyDto.Seq);
                return historyDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 추가 중 오류 발생: {@HistoryDto}", historyDto);
                throw;
            }
        }

        public async Task<IEnumerable<UnRegMarkerHistory>> GetVisitHistoryById(string OpnSfTeamCode, string MgtNo)
        {
            try
            {
                _logger.LogInformation("미등록 안경원 히스토리 데이터 조회 시작: MgtNo={MgtNo}", MgtNo);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@OpnSfTeamCode", OpnSfTeamCode);
                parameters.Add("@MgtNo", MgtNo);

                var unRegOpticianHistories = await connection.QueryAsync<UnRegMarkerHistory>(
                    "GetUnRegOpticianHistory",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("미등록 안경원 히스토리 데이터 조회 완료: MgtNo={MgtNo}, Count={Count}", MgtNo, unRegOpticianHistories?.Count() ?? 0);
                return unRegOpticianHistories ?? Enumerable.Empty<UnRegMarkerHistory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "미등록 안경원 히스토리 데이터 조회 중 오류 발생: MgtNo={MgtNo}", MgtNo);
                throw;
            }
        }
    }
}
