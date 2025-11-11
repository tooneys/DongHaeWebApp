
using Dapper;
using System.Data;
using WebApi.DTOs;
using WebApi.Infrastructure;

namespace WebApi.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IDbConnectionFactory connectionFactory, ILogger<ReportService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GenerateSalesReportAsync(
            DateTime startDate, 
            DateTime endDate, 
            string type = "", 
            string? customer = null, 
            string? manager = null
        )
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                
                int searchType = 0;
                if (type == "전체")
                {
                    searchType = 0;
                }
                else if (type == "매출")
                {
                    searchType = 1;
                }
                else if (type == "입금")
                {
                    searchType = 2;
                }
                else if (type == "잔액")
                {
                    searchType = 3;
                }

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01", DbType.String);
                parameters.Add("@CD_BUSIDIV", "001", DbType.String);
                parameters.Add("@DT_COMP_F", startDate.ToString("yyyyMMdd"), DbType.String);
                parameters.Add("@DT_COMP_T", endDate.ToString("yyyyMMdd"), DbType.String);
                parameters.Add("@CD_EMP", "", DbType.String);
                parameters.Add("@CD_CUST", "", DbType.String);
                parameters.Add("@NM_CUST", "", DbType.String);
                parameters.Add("@OP_GUBUN", searchType, DbType.Int32);
                parameters.Add("@CD_CUSTDIV", "", DbType.String);

                var response = await connection.QueryAsync(
                    "SP_SMVW_SALESCUST_SV",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                );

                var records = response.Cast<IDictionary<string, object>>().Select(row => row.ToDictionary(k => k.Key, v => v.Value))
                    .ToList();

                return records ?? Enumerable.Empty<Dictionary<string, object>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"거래처별매출현황 조회 중 오류 발생: manager={manager}");
                throw;
            }
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GenerateUserPlanReportAsync(
            string searchMonth, 
            string manager, 
            string? valueDiv, 
            string? itemDiv
        )
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                int valueDivCode = valueDiv switch
                {
                    "전체" => 0,
                    "수량" => 1,
                    "금액" => 2,
                    _ => 0
                };

                string itemDivCode = itemDiv switch
                {
                    "전체" => "",
                    "RX" => "001",
                    "ST" => "002",
                    "기타" => "003",
                    _ => ""
                };

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01", DbType.String);
                parameters.Add("@CD_BUSIDIV", "001", DbType.String);
                parameters.Add("@CD_EMP", manager, DbType.String);
                parameters.Add("@DT_YYYYMM", searchMonth, DbType.String);
                parameters.Add("@CD_VALUEGB", valueDivCode, DbType.Int32);
                parameters.Add("@CD_ITEMDIV", itemDivCode, DbType.String);

                var response = await connection.QueryAsync(
                    "SP_SMPL_USERPLAN_SV",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                );

                var records = response.Cast<IDictionary<string, object>>().Select(row => row.ToDictionary(k => k.Key, v => v.Value))
                    .ToList();

                return records ?? Enumerable.Empty<Dictionary<string, object>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"계획별주문현황 조회 중 오류 발생: searchMonth={searchMonth}, manager={manager}");
                throw;
            }
        }

        public async Task<UserPlanTargetReportDto> GetUserPlanTargetReportAsync(string searchMonth, string manager = "")
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01", DbType.String);
                parameters.Add("@CD_BUSIDIV", "001", DbType.String);
                parameters.Add("@CD_EMP", manager, DbType.String);
                parameters.Add("@DT_YYYYMM", searchMonth, DbType.String);

                var response = await connection.QueryFirstAsync<UserPlanTargetReportDto>(
                    "SP_SMPL_USERPLANTARGET_SV",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                );

                if (response == null)
                {
                    _logger.LogWarning($"계획별주문현황(달성율) 데이터가 없습니다: searchMonth={searchMonth}, manager={manager}");
                    return new UserPlanTargetReportDto();
                }
                _logger.LogInformation($"계획별주문현황(달성율) 조회 성공: searchMonth={searchMonth}, manager={manager}");

                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"계획별주문현황(달성율) 조회 중 오류 발생: searchMonth={searchMonth}, manager={manager}");
                throw;
            }
        }
    }
}
