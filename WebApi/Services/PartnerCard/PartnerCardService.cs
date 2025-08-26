using Dapper;
using System.Data;
using WebApi.Models;
using WebApi.Infrastructure;

namespace WebApi.Services.PartnerCard
{
    public class PartnerCardService : IPartnerCardService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<PartnerCardService> _logger;

        public PartnerCardService(IDbConnectionFactory connectionFactory, ILogger<PartnerCardService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<PartnerCardDetail> GetPartnerCardDetailById(string opticianId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);

                var partner = await connection.QueryFirstOrDefaultAsync<PartnerCardDetail>(
                    "GetPartnerCardById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return partner ?? new PartnerCardDetail();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파트너카드 상세 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<CustNote>> GetCustNotesById(string opticianId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);

                var partner = await connection.QueryAsync<CustNote>(
                    "GetCustNotesById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return partner ?? Enumerable.Empty<CustNote>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "고객 노트 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<OpticianPromotion>> GetOpticianPromotionById(string opticianId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CUST", opticianId);

                var promotions = await connection.QueryAsync<OpticianPromotion>(
                    "GetOpticianPromotionById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return promotions ?? Enumerable.Empty<OpticianPromotion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 프로모션 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersById(string opticianId, int year)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", opticianId);
                parameters.Add("@DT_YYYY", year);

                var orders = await connection.QueryAsync<OrderDto>(
                    "SP_SMVW_PARTNERCARD_ACCT_S",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return orders ?? Enumerable.Empty<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "주문 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersById(string opticianId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                string dateFrom = DateTime.Now.ToString("yyyy0101");
                string dateTo = DateTime.Now.ToString("yyyyMMdd");

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", opticianId);
                parameters.Add("@CD_ITEMDIV", "");
                //parameters.Add("@DT_COMP_F", dateFrom);
                //parameters.Add("@DT_COMP_T", dateTo);

                var orders = await connection.QueryAsync<SalesOrderDto>(
                    "SP_SMVW_PARTNERCARD_ACCT_S2",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "판매 주문 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<ReturnOrderDto>> GetReturnOrdersById(string opticianId)
        {
            try
            {
                _logger.LogInformation("파트너카드 반품정보 조회 시작: OpticianId={OpticianId}", opticianId);

                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", opticianId);

                var orders = await connection.QueryAsync<ReturnOrderDto>(
                    "SP_API_SMCR_RETURN_PSV",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("파트너카드 반품정보 조회 완료: OpticianId={OpticianId}, Count={Count}", opticianId, orders?.Count() ?? 0);
                return orders ?? Enumerable.Empty<ReturnOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "반품 주문 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<OpticianHistoryDto>> GetOpticianHistoriesById(string opticianId)
        {
            try
            {
                _logger.LogInformation("파트너카드 이력정보 조회 시작: OpticianId={OpticianId}", opticianId);

                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", opticianId);

                var orders = await connection.QueryAsync<OpticianHistoryDto>(
                    "SP_SMVW_PARTNERCARD_VISIT_S",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("파트너카드 이력정보 조회 완료: OpticianId={OpticianId}, Count={Count}", opticianId, orders?.Count() ?? 0);
                return orders ?? Enumerable.Empty<OpticianHistoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 이력 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<IEnumerable<OpticianClaimDto>> GetOpticianClaimsById(string opticianId)
        {
            try
            {
                _logger.LogInformation("클레임 정보 조회 시작: OpticianId={OpticianId}", opticianId);

                using var connection = _connectionFactory.CreateConnection();

                // 파라미터 생성
                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", opticianId);
                parameters.Add("@DT_COMP_F", "");
                parameters.Add("@DT_COMP_T", "");

                var claims = await connection.QueryAsync<OpticianClaimDto>(
                    "SP_SMVW_PARTNERCARD_CLAIM_S",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("클레임 정보 조회 완료: OpticianId={OpticianId}, Count={Count}", opticianId, claims?.Count() ?? 0);
                return claims ?? Enumerable.Empty<OpticianClaimDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "클레임 이력 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                throw;
            }
        }

        public async Task<OpticianHistoryDto> AddVisitHistoryAsync(OpticianHistoryDto historyDto)
        {
            try
            {
                _logger.LogInformation("방문이력 추가 시작: OpticianId={OpticianId}", historyDto.CD_CUST);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", historyDto.CD_CUST);
                parameters.Add("@DT_COMP", historyDto.DT_COMP.Replace("-", ""));
                parameters.Add("@TX_REASON", historyDto.TX_REASON);
                parameters.Add("@TX_PURPOSE", historyDto.TX_PURPOSE);
                parameters.Add("@TX_NOTE", historyDto.TX_NOTE);
                parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "SP_SMVW_PARTNERCARD_VISIT_API_I", // 방문이력 추가 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                historyDto.ID = parameters.Get<int>("@NewId");

                _logger.LogInformation("방문이력 추가 완료: Id={Id}, OpticianId={OpticianId}", historyDto.ID, historyDto.CD_CUST);
                return historyDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 추가 중 오류 발생: {@HistoryDto}", historyDto);
                throw;
            }
        }

        public async Task<OpticianHistoryDto> GetVisitHistoryByIdAsync(int historyId)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@ID", historyId);

                var result = await connection.QueryFirstOrDefaultAsync<OpticianHistoryDto>(
                    "SP_SMVW_PARTNERCARD_VISIT_S_BY_ID", // ID로 단건 조회 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 단건 조회 중 오류 발생: HistoryId={HistoryId}", historyId);
                throw;
            }
        }

        public async Task<OpticianHistoryDto> UpdateVisitHistoryAsync(OpticianHistoryDto historyDto)
        {
            try
            {
                _logger.LogInformation("방문이력 수정 시작: Id={Id}", historyDto.ID);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@ID", historyDto.ID);
                parameters.Add("@DT_COMP", historyDto.DT_COMP);
                parameters.Add("@TX_REASON", historyDto.TX_REASON);
                parameters.Add("@TX_PURPOSE", historyDto.TX_PURPOSE);
                parameters.Add("@TX_NOTE", historyDto.TX_NOTE);

                var affectedRows = await connection.ExecuteAsync(
                    "SP_SMVW_PARTNERCARD_VISIT_U", // 방문이력 수정 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (affectedRows > 0)
                {
                    _logger.LogInformation("방문이력 수정 완료: Id={Id}", historyDto.ID);
                    return historyDto;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 수정 중 오류 발생: Id={Id}", historyDto.ID);
                throw;
            }
        }

        public async Task<bool> DeleteVisitHistoryAsync(int historyId)
        {
            try
            {
                _logger.LogInformation("방문이력 삭제 시작: Id={Id}", historyId);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@Id", historyId);

                var affectedRows = await connection.ExecuteAsync(
                    "SP_SMVW_PARTNERCARD_VISIT_D", // 방문이력 삭제 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var isDeleted = affectedRows > 0;
                if (isDeleted)
                {
                    _logger.LogInformation("방문이력 삭제 완료: Id={Id}", historyId);
                }

                return isDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 삭제 중 오류 발생: HistoryId={HistoryId}", historyId);
                throw;
            }
        }
        
        public async Task<OpticianPromotion> AddPromotionAsync(OpticianPromotion promotion)
        {
            try
            {
                _logger.LogInformation("판촉물 데이터베이스 저장 시작: CustCode={CustCode}", promotion.CustCode);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01");
                parameters.Add("@CD_BUSIDIV", "001");
                parameters.Add("@CD_CUST", promotion.CustCode);
                parameters.Add("@DT_COMP", promotion.RegDate?.ToString("yyyyMMdd"));
                parameters.Add("@TX_PROMOTION", promotion.Promotion);
                parameters.Add("@ImageUrl", promotion.ImageUrl);
                parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "SP_SMVW_PARTNERCARD_PROMOTION_API_I", // 방문이력 추가 SP
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                promotion.Id = parameters.Get<int>("@NewId");

                _logger.LogInformation($"판촉물 데이터베이스 저장 완료: Id={promotion.Id}");

                return promotion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"판촉물 데이터베이스 저장 중 오류: CustCode={promotion.CustCode}");
                throw;
            }
        }

        public async Task UpdateOpticianStoreImage(OpticianStoreImage storeImage)
        {
            try
            {
                _logger.LogInformation($"매장이미지 데이터베이스 저장 시작: OpticianId={storeImage.OpticianId}");

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@CD_CORP", "01", DbType.String);
                parameters.Add("@CD_CUST", storeImage.OpticianId, DbType.String);
                parameters.Add("@Slot", storeImage.ImageSlot);
                parameters.Add("@ImageUrl", storeImage.ImageUrl, DbType.String);

                await connection.ExecuteAsync(
                    "SP_BSCT_CUST_IMG_U",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation($"매장이미지 데이터베이스 저장 완료: OpticianId={storeImage.OpticianId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"매장이미지 데이터베이스 저장 중 오류: OpticianId={storeImage.OpticianId}");
                throw;
            }
        }
    }
}
