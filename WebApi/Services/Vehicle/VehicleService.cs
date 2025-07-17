using Dapper;
using System.Data;
using System.Reflection.Emit;
using WebApi.DTOs;
using WebApi.Infrastructure;

namespace WebApi.Services.Vehicle
{
    public class VehicleService : IVehicleService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(IDbConnectionFactory connectionFactory, ILogger<VehicleService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesByIdAsync(string empCode)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_EMP", empCode, DbType.String);

                var response = await connection.QueryAsync<VehicleDto>(
                    "GETVEHICLESBYID",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return response ?? Enumerable.Empty<VehicleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"차량일지 조회 중 오류 발생: empCode={empCode}");
                throw;
            }
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicleDto)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_EMP", vehicleDto.CD_EMP, DbType.String);
                parameters.Add("@NO_CAR", vehicleDto.NO_CAR, DbType.String);
                parameters.Add("@DT_COMP", vehicleDto.DT_COMP?.ToString("yyyyMMdd"), DbType.String);
                parameters.Add("@TX_AREA", vehicleDto.TX_AREA, DbType.String);
                parameters.Add("@VL_BEFORE", vehicleDto.VL_BEFORE, DbType.Decimal);
                parameters.Add("@VL_AFTER", vehicleDto.VL_AFTER, DbType.Decimal);
                parameters.Add("@VL_DISTANCE", vehicleDto.VL_DISTANCE, DbType.Decimal);
                parameters.Add("@AM_OIL", vehicleDto.VL_FUEL, DbType.Decimal);
                parameters.Add("@CD_INSUSER", vehicleDto.CD_EMP, DbType.String);
                parameters.Add("@CD_UPDUSER", vehicleDto.CD_EMP, DbType.String);
                parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var response = await connection.ExecuteAsync(
                    "AddVehicle",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                vehicleDto.SEQ = parameters.Get<int>("@NewId");

                _logger.LogInformation($"차량일지 데이터베이스 저장 완료: SEQ={vehicleDto.SEQ}");

                return vehicleDto ?? new VehicleDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"차량일지 저장 중 오류 발생: NO_CAR={vehicleDto.NO_CAR}");
                throw;
            }
        }

    }
}
