using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using WebApi.Infrastructure;
using WebApi.Models;

namespace WebApi.Repositories
{
    public interface IMarketingDataRepository
    {
        Task<int> CreateAsync(MarketingData data);
        Task<MarketingData?> GetByIdAsync(int id);
        Task<IEnumerable<MarketingData>> GetAllAsync();
        Task<bool> UpdateAsync(MarketingData data);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    public class MarketingDataRepository : IMarketingDataRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<MarketingDataRepository> _logger;

        public MarketingDataRepository(IDbConnectionFactory connectionFactory, ILogger<MarketingDataRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(MarketingData data)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new
                {
                    data.Name,
                    data.TabType,
                    data.Description,
                    data.CoverImageUrl,
                    data.DownloadFileUrl,
                };

                var result = await connection.QuerySingleAsync<int>(
                    "sp_InsertMarketingData",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                _logger.LogInformation("마케팅 데이터가 성공적으로 생성되었습니다. ID: {Id}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 생성 중 오류가 발생했습니다.");
                throw;
            }
        }

        public async Task<MarketingData?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new { Id = id };

                var result = await connection.QuerySingleOrDefaultAsync<MarketingData>(
                    "sp_GetMarketingDataById",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 조회 중 오류가 발생했습니다. ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MarketingData>> GetAllAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var results = await connection.QueryAsync<MarketingData>(
                    "sp_GetAllMarketingData",
                    commandType: CommandType.StoredProcedure);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 목록 조회 중 오류가 발생했습니다.");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(MarketingData data)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new
                {
                    data.Id,
                    data.Name,
                    data.TabType,
                    data.Description,
                    data.CoverImageUrl,
                    data.DownloadFileUrl
                };

                var affectedRows = await connection.ExecuteAsync(
                    "sp_UpdateMarketingData",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var success = affectedRows > 0;
                if (success)
                {
                    _logger.LogInformation("마케팅 데이터가 성공적으로 수정되었습니다. ID: {Id}", data.Id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 수정 중 오류가 발생했습니다. ID: {Id}", data.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new { Id = id };

                var affectedRows = await connection.QuerySingleAsync<int>(
                    "sp_DeleteMarketingData",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var success = affectedRows > 0;
                if (success)
                {
                    _logger.LogInformation("마케팅 데이터가 성공적으로 삭제되었습니다. ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("삭제할 마케팅 데이터를 찾을 수 없습니다. ID: {Id}", id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 삭제 중 오류가 발생했습니다. ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var data = await GetByIdAsync(id);
                return data != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "마케팅 데이터 존재 여부 확인 중 오류가 발생했습니다. ID: {Id}", id);
                throw;
            }
        }
    }
}
