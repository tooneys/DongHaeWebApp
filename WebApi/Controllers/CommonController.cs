using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommonController : ControllerBase
    {
        private readonly ICommonService _commonService;
        private readonly ILogger<CommonController> _logger;

        public CommonController(ICommonService commonService, ILogger<CommonController> logger)
        {
            _commonService = commonService;
            _logger = logger;
        }

        #region 공통 검증 메서드

        private IActionResult ValidateOpticianId(string opticianId)
        {
            if (string.IsNullOrWhiteSpace(opticianId))
                return BadRequest(new ApiErrorResponse("안경사 ID가 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        #endregion

        /// <summary>
        /// 모든 안경사 목록을 조회합니다.
        /// </summary>
        /// <returns>안경사 목록</returns>
        [HttpGet("opticians")]
        [ProducesResponseType(typeof(ApiResponse<List<Optician>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpticiansAsync()
        {
            try
            {
                _logger.LogInformation("안경사 목록 조회 시작");

                var opticians = await _commonService.GetOpticiansAsync();

                if (opticians == null || !opticians.Any())
                {
                    _logger.LogWarning("안경사 목록이 없습니다.");
                    return NotFound(new ApiErrorResponse("안경사 목록이 없습니다.", "NOT_FOUND"));
                }

                _logger.LogInformation("안경사 목록 조회 완료: Count={Count}", opticians.Count);

                return Ok(new ApiResponse<List<Optician>>
                {
                    Message = "안경사 목록 조회가 완료되었습니다.",
                    Data = opticians,
                    TotalCount = opticians.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 목록 조회 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "안경사 목록 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 특정 안경사 정보를 조회합니다.
        /// </summary>
        /// <param name="opticianId">안경사 ID</param>
        /// <returns>안경사 정보</returns>
        [HttpGet("opticians/{opticianId}")]
        [ProducesResponseType(typeof(ApiResponse<Optician>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpticiansByIdAsync(string opticianId)
        {
            try
            {
                var validationResult = ValidateOpticianId(opticianId);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("안경사 조회 시작: OpticianId={OpticianId}", opticianId);

                var optician = await _commonService.GetOpticiansByIdAsync(opticianId);

                if (optician == null)
                {
                    _logger.LogWarning("안경사를 찾을 수 없음: OpticianId={OpticianId}", opticianId);
                    return NotFound(new ApiErrorResponse("안경사를 찾을 수 없습니다.", "NOT_FOUND"));
                }

                _logger.LogInformation("안경사 조회 완료: OpticianId={OpticianId}", opticianId);

                return Ok(new ApiResponse<Optician>
                {
                    Message = "안경사 조회가 완료되었습니다.",
                    Data = optician
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "안경사 조회 중 오류 발생: OpticianId={OpticianId}", opticianId);
                return StatusCode(500, new ApiErrorResponse(
                    "안경사 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 코드 목록을 조회합니다.
        /// </summary>
        /// <param name="codeType">코드 타입</param>
        /// <returns>코드 목록</returns>
        [HttpGet("codes/{codeType}")]
        [ProducesResponseType(typeof(ApiResponse<List<CodeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCodeListAsync(string codeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codeType))
                    return BadRequest(new ApiErrorResponse("코드 타입이 필요합니다.", "VALIDATION_ERROR"));

                _logger.LogInformation("코드 목록 조회 시작: CodeType={CodeType}", codeType);

                var codes = await _commonService.GetCodeListAsync(codeType);

                _logger.LogInformation("코드 목록 조회 완료: CodeType={CodeType}, Count={Count}",
                    codeType, codes?.Count ?? 0);

                return Ok(new ApiResponse<List<CodeDto>>
                {
                    Message = "코드 목록 조회가 완료되었습니다.",
                    Data = codes ?? new List<CodeDto>(),
                    TotalCount = codes?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "코드 목록 조회 중 오류 발생: CodeType={CodeType}", codeType);
                return StatusCode(500, new ApiErrorResponse(
                    "코드 목록 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 지역 목록을 조회합니다.
        /// </summary>
        /// <returns>지역 목록</returns>
        [HttpGet("regions")]
        [ProducesResponseType(typeof(ApiResponse<List<RegionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRegionsAsync()
        {
            try
            {
                _logger.LogInformation("지역 목록 조회 시작");

                var regions = await _commonService.GetRegionsAsync();

                _logger.LogInformation("지역 목록 조회 완료: Count={Count}", regions?.Count ?? 0);

                return Ok(new ApiResponse<List<RegionDto>>
                {
                    Message = "지역 목록 조회가 완료되었습니다.",
                    Data = regions ?? new List<RegionDto>(),
                    TotalCount = regions?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "지역 목록 조회 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "지역 목록 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        [HttpGet("salesemp")]
        [ProducesResponseType(typeof(ApiResponse<List<EmplyeeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSalesEmpAsync()
        {
            try
            {
                _logger.LogInformation("영업사원 목록 조회 시작");

                var regions = await _commonService.GetEmployeeAsync();

                _logger.LogInformation("영업사원 목록 조회 완료: Count={Count}", regions?.Count ?? 0);

                return Ok(new ApiResponse<List<EmplyeeDto>>
                {
                    Message = "영업사원 목록 조회가 완료되었습니다.",
                    Data = regions ?? new List<EmplyeeDto>(),
                    TotalCount = regions?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "영업사원 목록 조회 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "영업사원 목록 조회 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }
    }
}
