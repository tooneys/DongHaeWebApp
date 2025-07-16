using Dapper;
using System.Data;
using WebApi.DTOs;
using WebApi.Infrastructure;

namespace WebApi.Services.Dashboard
{
    public class DashboardSalesService : IDashboardSalesService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<DashboardSalesService> _logger;

        public DashboardSalesService(IDbConnectionFactory connectionFactory, ILogger<DashboardSalesService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<List<OpticalStoreSalesDto>> GetTopStoresSalesAsync(int count, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Count", count, DbType.Int32);
                parameters.Add("@UserId", userId, DbType.String);

                var result = await connection.QueryAsync<OpticalStoreSalesDto>(
                    "GetTopStoresSales",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting top stores sales with count: {Count}", count);
                throw;
            }
        }

        public async Task<List<OpticalStoreSalesDto>> GetCurrentMonthSalesAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId, DbType.String);

                var result = await connection.QueryAsync<OpticalStoreSalesDto>(
                    "GetCurrentMonthSales",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current month sales");
                throw;
            }
        }

        public async Task<List<OpticalStoreSalesDeclineDto>> GetCurrentMonthSalesDeclineAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId, DbType.String);

                var result = await connection.QueryAsync<OpticalStoreSalesDeclineDto>(
                    "GetCurrentMonthSalesDecline",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current month sales decline");
                throw;
            }
        }

        public async Task<List<ItemGroupSalesDto>> GetItemGroupSalesAsync(string userId, CancellationToken cancellationToken = default)
        {

            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId, DbType.String);

                var result = await connection.QueryAsync<ItemGroupSalesDto>(
                    "GetItemGroupSales",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting item group sales");
                throw;
            }
        }
    }
}
