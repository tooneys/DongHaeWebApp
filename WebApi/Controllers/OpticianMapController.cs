using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services.Common;
using WebApi.Services.OpticianMap;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpticianMapController : ControllerBase
    {
        private readonly ICommonService _commonService;
        private readonly IOpticianMapService _opticianMapService;
        private readonly ILogger<OpticianMapController> _logger;

        public OpticianMapController(
            ICommonService commonService,
            IOpticianMapService opticianMapService,
            ILogger<OpticianMapController> logger)
        {
            _commonService = commonService;
            _opticianMapService = opticianMapService;
            _logger = logger;
        }
        
        private IActionResult ValidateVisitHistory(UnRegMarkerHistory historyDto)
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
        
        #region 공통 검증 메서드

        private IActionResult ValidateRegion(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                return BadRequest(new ApiErrorResponse("지역명이 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        private IActionResult ValidateId(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiErrorResponse("유효한 ID가 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }
        
        #endregion

        /// <summary>
        /// 모든 안경사 위치 정보를 조회합니다.
        /// </summary>
        /// <returns>안경사 위치 정보 목록</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OpticianGeoLocation>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpticianMapAll()
        {
            try
            {
                _logger.LogInformation("전체 안경사 위치 정보 조회 시작");

                var opticianLocations = await _opticianMapService.GetOpticianMapAll();

                if (opticianLocations == null || !opticianLocations.Any())
                {
                    _logger.LogWarning("안경사 위치 정보가 없습니다.");
                    return NotFound(new ApiErrorResponse("안경사 위치 정보가 없습니다.", "NOT_FOUND"));
                }

                var opticianList = opticianLocations.ToList();
                _logger.LogInformation("안경사 위치 정보 조회 완료: Count={Count}", opticianList.Count);

                return Ok(new ApiResponse<List<OpticianGeoLocation>>
                {
                    Message = "안경사 위치 정보 조회가 완료되었습니다.",
                    Data = opticianList,
                    TotalCount = opticianList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 위치 정보 조회 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "안경사 위치 정보 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 특정 지역의 안경사 위치 정보를 조회합니다.
        /// </summary>
        /// <param name="region">지역명</param>
        /// <returns>해당 지역의 안경사 위치 정보 목록</returns>
        [HttpGet("region/{region}")]
        [ProducesResponseType(typeof(ApiResponse<List<OpticianGeoLocation>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpticianMapByRegion(string region)
        {
            try
            {
                var validationResult = ValidateRegion(region);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("지역별 안경사 위치 정보 조회 시작: Region={Region}", region);

                var opticianLocations = await _opticianMapService.GetOpticianMapByRegion(region);

                if (opticianLocations == null || !opticianLocations.Any())
                {
                    _logger.LogWarning("해당 지역에 안경사 위치 정보가 없습니다: Region={Region}", region);
                    return NotFound(new ApiErrorResponse($"{region} 지역에 안경사 위치 정보가 없습니다.", "NOT_FOUND"));
                }

                var opticianList = opticianLocations.ToList();
                _logger.LogInformation("지역별 안경사 위치 정보 조회 완료: Region={Region}, Count={Count}", region, opticianList.Count);

                return Ok(new ApiResponse<List<OpticianGeoLocation>>
                {
                    Message = $"{region} 지역의 안경사 위치 정보 조회가 완료되었습니다.",
                    Data = opticianList,
                    TotalCount = opticianList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "지역별 안경사 위치 정보 조회 중 오류 발생: Region={Region}", region);
                return StatusCode(500, new ApiErrorResponse(
                    "지역별 안경사 위치 정보 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 특정 안경사의 위치 정보를 조회합니다.
        /// </summary>
        /// <param name="id">안경사 ID</param>
        /// <returns>안경사 위치 정보</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<OpticianGeoLocation>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpticianById(int id)
        {
            try
            {
                var validationResult = ValidateId(id);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("안경사 위치 정보 조회 시작: Id={Id}", id);

                var optician = await _opticianMapService.GetOpticianLocationById(id.ToString());

                if (optician == null)
                {
                    _logger.LogWarning("안경사 위치 정보를 찾을 수 없음: Id={Id}", id);
                    return NotFound(new ApiErrorResponse("안경사 위치 정보를 찾을 수 없습니다.", "NOT_FOUND"));
                }

                _logger.LogInformation("안경사 위치 정보 조회 완료: Id={Id}", id);

                return Ok(new ApiResponse<OpticianGeoLocation>
                {
                    Message = "안경사 위치 정보 조회가 완료되었습니다.",
                    Data = optician
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 위치 정보 조회 중 오류 발생: Id={Id}", id);
                return StatusCode(500, new ApiErrorResponse(
                    "안경사 위치 정보 조회 중 오류가 발생했습니다.",
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
        [ProducesResponseType(typeof(ApiResponse<UnRegMarkerHistory>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddVisitHistory([FromBody] UnRegMarkerHistory historyDto)
        {
            try
            {
                var validationResult = ValidateVisitHistory(historyDto);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("방문이력 추가 시작: OpticianId={OpticianId}", historyDto.MgtNo);

                var addedHistory = await _opticianMapService.AddVisitHistoryAsync(historyDto);

                return Ok(new ApiResponse<UnRegMarkerHistory>
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
        /// 모든 안경사 위치 정보를 조회합니다.
        /// </summary>
        /// <returns>안경사 위치 정보 목록</returns>
        [HttpGet("visit-history")]
        [ProducesResponseType(typeof(ApiResponse<List<UnRegMarkerHistory>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVisitHistoryById([FromQuery] string OpnSfTeamCode,[FromQuery] string MgtNo)
        {
            try
            {
                _logger.LogInformation("미등록 안경원 방문이력 정보 조회 시작");

                var opticianLocations = await _opticianMapService.GetVisitHistoryById(OpnSfTeamCode, MgtNo);

                if (opticianLocations == null || !opticianLocations.Any())
                {
                    _logger.LogWarning("미등록 안경원 방문이력 정보가 없습니다.");
                    return NotFound(new ApiErrorResponse("미등록 안경원 방문이력 정보가 없습니다.", "NOT_FOUND"));
                }

                var opticianList = opticianLocations.ToList();
                _logger.LogInformation("미등록 안경원 방문이력 정보 조회 완료: Count={Count}", opticianList.Count);

                return Ok(new ApiResponse<List<UnRegMarkerHistory>>
                {
                    Message = "미등록 안경원 방문이력 정보 조회가 완료되었습니다.",
                    Data = opticianList,
                    TotalCount = opticianList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "미등록 안경원 방문이력 정보 조회 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "미등록 안경원 방문이력 정보 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }
    }
}
