using Dapper;
using System.Data;
using WebApi.Models;
using WebApi.Infrastructure;

namespace WebApi.Services.Common
{
    public class CommonService : ICommonService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CommonService> _logger;

        public CommonService(IDbConnectionFactory connectionFactory, ILogger<CommonService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public Task<List<CodeDto>> GetCodeListAsync(string codeType)
        {
            throw new NotImplementedException();
        }

        public async Task<List<EmplyeeDto>> GetEmployeeAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var opticians = await connection.QueryAsync<EmplyeeDto>(
                    "GetSalesEmployee",
                    commandType: CommandType.StoredProcedure
                );

                return opticians.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 목록 조회 중 오류 발생");
                throw;
            }
        }

        public async Task<List<Optician>> GetOpticiansAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var opticians = await connection.QueryAsync<Optician>(
                    "SP_GetOpticians",
                    commandType: CommandType.StoredProcedure
                );

                return opticians.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 목록 조회 중 오류 발생");
                throw;
            }
        }

        public async Task<Optician> GetOpticiansByIdAsync(string opticianId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);

                var opticians = await connection.QueryAsync<Optician>(
                    "SP_GetOpticians",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return opticians.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public Task<List<RegionDto>> GetRegionsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
