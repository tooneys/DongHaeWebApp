using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Models;
using WebApi.Services.Common;
using WebApi.Services.PartnerCard;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PartnerCardController : ControllerBase
    {
        private readonly IPartnerCardService _partnerCardService;
        private readonly IImageService _imageService;
        private readonly ICommonService _commonService;
        private readonly ILogger<PartnerCardController> _logger;

        public PartnerCardController(
            IPartnerCardService partnerCardService,
            IImageService imageService,
            ICommonService commonService,
            ILogger<PartnerCardController> logger)
        {
            _partnerCardService = partnerCardService;
            _imageService = imageService;
            _commonService = commonService;
            _logger = logger;
        }

        #region 공통 검증 메서드들

        private IActionResult ValidateOpticianId(string opticianId)
        {
            if (string.IsNullOrWhiteSpace(opticianId))
                return BadRequest(new ApiErrorResponse("안경사 ID가 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        private IActionResult ValidateVisitHistory(OpticianHistoryDto historyDto)
        {
            if (historyDto == null)
                return BadRequest(new ApiErrorResponse("방문이력 정보가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(historyDto.TX_REASON))
                return BadRequest(new ApiErrorResponse("방문사유는 필수입니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(historyDto.TX_PURPOSE))
                return BadRequest(new ApiErrorResponse("방문목적은 필수입니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(historyDto.DT_COMP))
                return BadRequest(new ApiErrorResponse("방문일자는 필수입니다.", "VALIDATION_ERROR"));

            if (!DateTime.TryParse(historyDto.DT_COMP, out var visitDate))
                return BadRequest(new ApiErrorResponse("올바른 날짜 형식이 아닙니다. (yyyy-MM-dd)", "VALIDATION_ERROR"));

            if (visitDate > DateTime.Today)
                return BadRequest(new ApiErrorResponse("방문일자는 오늘 이후로 설정할 수 없습니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        private IActionResult ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new ApiErrorResponse("시작일은 종료일보다 이전이어야 합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        private IActionResult ValidateHistoryId(int historyId)
        {
            if (historyId <= 0)
                return BadRequest(new ApiErrorResponse("유효한 방문이력 ID가 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        private IEnumerable<OpticianHistoryDto> ApplyFilters(
            IEnumerable<OpticianHistoryDto> histories,
            DateTime? startDate,
            DateTime? endDate,
            string searchTerm)
        {
            var filtered = histories;

            // 날짜 필터 적용
            if (startDate.HasValue)
            {
                filtered = filtered.Where(h =>
                    DateTime.TryParse(h.DT_COMP, out var date) && date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filtered = filtered.Where(h =>
                    DateTime.TryParse(h.DT_COMP, out var date) && date <= endDate.Value);
            }

            // 검색어 필터 적용
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(h =>
                    (h.TX_REASON?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (h.TX_PURPOSE?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (h.TX_NOTE?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true));
            }

            return filtered.OrderByDescending(h =>
                DateTime.TryParse(h.DT_COMP, out var date) ? date : DateTime.MinValue);
        }

        #endregion

        /// <summary>
        /// 파트너카드 정보를 조회합니다.
        /// </summary>
        /// <param name="opticianId">안경사 ID</param>
        /// <returns>파트너카드 정보</returns>
        [HttpGet("{opticianId}")]
        [ProducesResponseType(typeof(ApiResponse<PartnerCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPartnerCardById(string opticianId)
        {
            try
            {
                var validationResult = ValidateOpticianId(opticianId);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("파트너카드 조회 시작: OpticianId={OpticianId}", opticianId);

                // Factory 패턴을 사용하므로 안전한 병렬 처리 가능
                var opticianTask = _commonService.GetOpticiansByIdAsync(opticianId);
                var partnerCardDetailTask = _partnerCardService.GetPartnerCardDetailById(opticianId);
                var custNotesTask = _partnerCardService.GetCustNotesById(opticianId);
                var promotionsTask = _partnerCardService.GetOpticianPromotionById(opticianId);
                var ordersTask = _partnerCardService.GetOrdersById(opticianId);
                var salesOrdersTask = _partnerCardService.GetSalesOrdersById(opticianId);
                var returnOrdersTask = _partnerCardService.GetReturnOrdersById(opticianId);
                var opticianHistoriesTask = _partnerCardService.GetOpticianHistoriesById(opticianId);
                var claimsTask = _partnerCardService.GetOpticianClaimsById(opticianId);

                // 모든 작업을 병렬로 실행
                await Task.WhenAll(
                    opticianTask,
                    partnerCardDetailTask,
                    custNotesTask,
                    promotionsTask,
                    ordersTask,
                    salesOrdersTask,
                    returnOrdersTask,
                    opticianHistoriesTask,
                    claimsTask
                );

                var partnerCard = new PartnerCardDto
                {
                    Id = opticianId,
                    Optician = await opticianTask,
                    PartnerCardDetail = await partnerCardDetailTask,
                    CustNotes = (await custNotesTask)?.ToList() ?? new List<CustNote>(),
                    Promotions = (await promotionsTask)?.ToList() ?? new List<OpticianPromotion>(),
                    Orders = (await ordersTask)?.ToList() ?? new List<OrderDto>(),
                    SalesOrders = (await salesOrdersTask)?.ToList() ?? new List<SalesOrderDto>(),
                    ReturnOrders = (await returnOrdersTask)?.ToList() ?? new List<ReturnOrderDto>(),
                    OpticianHistories = (await opticianHistoriesTask)?.ToList() ?? new List<OpticianHistoryDto>(),
                    Claims = (await claimsTask)?.ToList() ?? new List<OpticianClaimDto>()
                };

                _logger.LogInformation("파트너카드 조회 완료: OpticianId={OpticianId}", opticianId);

                return Ok(new ApiResponse<PartnerCardDto>
                {
                    Message = "파트너카드 조회가 완료되었습니다.",
                    Data = partnerCard
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "파트너카드 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                return StatusCode(500, new ApiErrorResponse(
                    "파트너카드 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 새 방문이력을 추가합니다.
        /// </summary>
        /// <param name="historyDto">추가할 방문이력 정보</param>
        /// <returns>추가된 방문이력 정보</returns>
        [HttpPost("visit-history")]
        [ProducesResponseType(typeof(ApiResponse<OpticianHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddVisitHistory([FromBody] OpticianHistoryDto historyDto)
        {
            try
            {
                var validationResult = ValidateVisitHistory(historyDto);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("방문이력 추가 시작: OpticianId={OpticianId}", historyDto.CD_CUST);

                var addedHistory = await _partnerCardService.AddVisitHistoryAsync(historyDto);

                return Ok(new ApiResponse<OpticianHistoryDto>
                {
                    Message = "방문이력이 성공적으로 추가되었습니다.",
                    Data = addedHistory
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "방문이력 추가 중 인수 오류: {@HistoryDto}", historyDto);
                return BadRequest(new ApiErrorResponse(ex.Message, "ARGUMENT_ERROR"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 추가 중 오류 발생: {@HistoryDto}", historyDto);
                return StatusCode(500, new ApiErrorResponse(
                    "방문이력 추가 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 특정 안경사의 방문이력 목록을 조회합니다.
        /// </summary>
        /// <param name="opticianId">안경사 ID</param>
        /// <param name="startDate">조회 시작일 (선택사항)</param>
        /// <param name="endDate">조회 종료일 (선택사항)</param>
        /// <param name="searchTerm">검색어 (선택사항)</param>
        /// <returns>방문이력 목록</returns>
        [HttpGet("visit-history/{opticianId}")]
        [ProducesResponseType(typeof(ApiResponse<List<OpticianHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVisitHistoryById(
            string opticianId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                var opticianValidation = ValidateOpticianId(opticianId);
                if (opticianValidation != null)
                    return opticianValidation;

                var dateValidation = ValidateDateRange(startDate, endDate);
                if (dateValidation != null)
                    return dateValidation;

                _logger.LogInformation("방문이력 목록 조회 시작: OpticianId={OpticianId}, StartDate={StartDate}, EndDate={EndDate}, SearchTerm={SearchTerm}",
                    opticianId, startDate, endDate, searchTerm);

                var histories = await _partnerCardService.GetOpticianHistoriesById(opticianId);

                if (histories == null || !histories.Any())
                {
                    return Ok(new ApiResponse<List<OpticianHistoryDto>>
                    {
                        Message = "방문이력 조회가 완료되었습니다.",
                        Data = new List<OpticianHistoryDto>(),
                        TotalCount = 0
                    });
                }

                var filteredHistories = ApplyFilters(histories, startDate, endDate, searchTerm).ToList();

                _logger.LogInformation("방문이력 목록 조회 완료: OpticianId={OpticianId}, TotalCount={TotalCount}, FilteredCount={FilteredCount}",
                    opticianId, histories.Count(), filteredHistories.Count);

                return Ok(new ApiResponse<List<OpticianHistoryDto>>
                {
                    Message = "방문이력 조회가 완료되었습니다.",
                    Data = filteredHistories,
                    TotalCount = filteredHistories.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                return StatusCode(500, new ApiErrorResponse(
                    "방문이력 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 특정 방문이력을 조회합니다.
        /// </summary>
        /// <param name="historyId">방문이력 ID</param>
        /// <returns>방문이력 정보</returns>
        [HttpGet("visit-history/detail/{historyId:int}")]
        [ProducesResponseType(typeof(ApiResponse<OpticianHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVisitHistoryDetail(int historyId)
        {
            try
            {
                var validationResult = ValidateHistoryId(historyId);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("방문이력 상세 조회 시작: HistoryId={HistoryId}", historyId);

                var history = await _partnerCardService.GetVisitHistoryByIdAsync(historyId);

                if (history == null)
                    return NotFound(new ApiErrorResponse("존재하지 않는 방문이력입니다.", "NOT_FOUND"));

                return Ok(new ApiResponse<OpticianHistoryDto>
                {
                    Message = "방문이력 상세 조회가 완료되었습니다.",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 상세 조회 중 오류 발생: HistoryId={HistoryId}", historyId);
                return StatusCode(500, new ApiErrorResponse(
                    "방문이력 상세 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 방문이력을 수정합니다.
        /// </summary>
        /// <param name="historyId">방문이력 ID</param>
        /// <param name="historyDto">수정할 방문이력 정보</param>
        /// <returns>수정된 방문이력 정보</returns>
        [HttpPut("visit-history/{historyId:int}")]
        [ProducesResponseType(typeof(ApiResponse<OpticianHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateVisitHistory(int historyId, [FromBody] OpticianHistoryDto historyDto)
        {
            try
            {
                var historyIdValidation = ValidateHistoryId(historyId);
                if (historyIdValidation != null)
                    return historyIdValidation;

                var historyValidation = ValidateVisitHistory(historyDto);
                if (historyValidation != null)
                    return historyValidation;

                _logger.LogInformation("방문이력 수정 시작: HistoryId={HistoryId}", historyId);

                historyDto.ID = historyId;
                var updatedHistory = await _partnerCardService.UpdateVisitHistoryAsync(historyDto);

                if (updatedHistory == null)
                    return NotFound(new ApiErrorResponse("존재하지 않는 방문이력입니다.", "NOT_FOUND"));

                return Ok(new ApiResponse<OpticianHistoryDto>
                {
                    Message = "방문이력이 성공적으로 수정되었습니다.",
                    Data = updatedHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 수정 중 오류 발생: HistoryId={HistoryId}", historyId);
                return StatusCode(500, new ApiErrorResponse(
                    "방문이력 수정 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 방문이력을 삭제합니다.
        /// </summary>
        /// <param name="historyId">삭제할 방문이력 ID</param>
        /// <returns>삭제 결과</returns>
        [HttpDelete("visit-history/{historyId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteVisitHistory(int historyId)
        {
            try
            {
                var validationResult = ValidateHistoryId(historyId);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("방문이력 삭제 시작: HistoryId={HistoryId}", historyId);

                var isDeleted = await _partnerCardService.DeleteVisitHistoryAsync(historyId);

                if (isDeleted)
                {
                    return Ok(new ApiResponse("방문이력이 성공적으로 삭제되었습니다."));
                }
                else
                {
                    return NotFound(new ApiErrorResponse("존재하지 않는 방문이력입니다.", "NOT_FOUND"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "방문이력 삭제 중 오류 발생: HistoryId={HistoryId}", historyId);
                return StatusCode(500, new ApiErrorResponse(
                    "방문이력 삭제 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }
        
        #region 판촉물 관련 검증 메서드들

        private IActionResult ValidatePromotionData(string regDate, string promotion, IFormFile imageFile)
        {
            if (string.IsNullOrWhiteSpace(regDate))
                return BadRequest(new ApiErrorResponse("등록일자는 필수입니다.", "VALIDATION_ERROR"));

            if (!DateTime.TryParse(regDate, out var date))
                return BadRequest(new ApiErrorResponse("올바른 날짜 형식이 아닙니다. (yyyy-MM-dd)", "VALIDATION_ERROR"));

            if (date > DateTime.Today)
                return BadRequest(new ApiErrorResponse("등록일자는 오늘 이후로 설정할 수 없습니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(promotion))
                return BadRequest(new ApiErrorResponse("판촉물명은 필수입니다.", "VALIDATION_ERROR"));

            if (imageFile != null && imageFile.Length > 10 * 1024 * 1024)
                return BadRequest(new ApiErrorResponse("이미지 크기는 10MB를 초과할 수 없습니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        #endregion

        [HttpPost("promotion")]
        [ProducesResponseType(typeof(ApiResponse<OpticianPromotion>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPromotion([FromForm] PartnerCardInsertDTOs dto)
        {
            try
            {
                // 1. 기본 유효성 검증
                var opticianValidation = ValidateOpticianId(dto.OpticianId);
                if (opticianValidation != null)
                    return opticianValidation;

                var promotionValidation = ValidatePromotionData(dto.RegDate, dto.Promotion, dto.ImageFile);
                if (promotionValidation != null)
                    return promotionValidation;

                _logger.LogInformation($"판촉물 추가 시작: OpticianId={dto.OpticianId}, Promotion={dto.Promotion}");
                
                // 2. 이미지 처리
                string imageUrl = null;
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var (url, _) = await _imageService.ProcessImageAsync(dto.ImageFile);
                    imageUrl = url;
                }
                
                // 3. 판촉물 데이터 생성
                var newPromotion = new OpticianPromotion
                {
                    CustCode = dto.OpticianId,
                    RegDate = DateTime.TryParse(dto.RegDate, out DateTime date) ? date : DateTime.Now,
                    Promotion = dto.Promotion,
                    ImageUrl = imageUrl
                };

                // 4. 데이터베이스 저장
                var addedPromotion = await _partnerCardService.AddPromotionAsync(newPromotion);
                
                return Ok(new ApiResponse<OpticianPromotion>
                {
                    Message = "판촉물이 성공적으로 추가되었습니다.",
                    Data = addedPromotion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"판촉물 추가 중 오류 발생 : OpticianId : {dto.OpticianId}, Promotion : {dto.Promotion}");
                return StatusCode(500, new ApiErrorResponse("판촉물 추가 중 오류가 발생했습니다.", "INTERNAL_ERROR", ex.Message));
            }
        }
    }
}
