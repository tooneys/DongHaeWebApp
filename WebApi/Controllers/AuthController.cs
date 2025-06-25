using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services.Auth;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        #region 공통 검증 메서드

        private IActionResult ValidateAuthRequest(AuthRequest request)
        {
            if (request == null)
                return BadRequest(new ApiErrorResponse("요청 데이터가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new ApiErrorResponse("사용자 ID가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ApiErrorResponse("비밀번호가 필요합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }

        #endregion

        /// <summary>
        /// 사용자 로그인을 처리합니다.
        /// </summary>
        /// <param name="request">로그인 요청 정보</param>
        /// <returns>인증된 사용자 정보</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<UserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            try
            {
                var validationResult = ValidateAuthRequest(request);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("로그인 요청 시작: UserId={UserId}", request.UserId);

                var user = await _authService.Authenticate(request.UserId, request.Password);

                if (user == null)
                {
                    _logger.LogWarning("로그인 실패: 잘못된 자격 증명, UserId={UserId}", request.UserId);
                    return Unauthorized(new ApiErrorResponse(
                        "아이디 또는 비밀번호가 잘못되었습니다.",
                        "INVALID_CREDENTIALS"));
                }

                _logger.LogInformation("로그인 성공: UserId={UserId}, Username={Username}",
                    user.UserId, user.Username);

                return Ok(new ApiResponse<UserProfile>
                {
                    Message = "로그인이 성공했습니다.",
                    Data = user
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "로그인 중 인수 오류: UserId={UserId}", request?.UserId);
                return BadRequest(new ApiErrorResponse(ex.Message, "ARGUMENT_ERROR"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그인 중 오류 발생: UserId={UserId}", request?.UserId);
                return StatusCode(500, new ApiErrorResponse(
                    "로그인 처리 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// JWT 토큰의 유효성을 검증합니다.
        /// </summary>
        /// <param name="request">토큰 검증 요청</param>
        /// <returns>토큰 유효성 결과</returns>
        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new ApiErrorResponse("토큰이 필요합니다.", "VALIDATION_ERROR"));

                _logger.LogInformation("토큰 검증 요청");

                var isValid = await _authService.ValidateTokenAsync(request.Token);

                return Ok(new ApiResponse<bool>
                {
                    Message = isValid ? "토큰이 유효합니다." : "토큰이 유효하지 않습니다.",
                    Data = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 검증 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "토큰 검증 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 토큰에서 사용자 정보를 추출합니다.
        /// </summary>
        /// <param name="request">토큰 요청</param>
        /// <returns>사용자 정보</returns>
        [HttpPost("user-from-token")]
        [ProducesResponseType(typeof(ApiResponse<UserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserFromToken([FromBody] TokenValidationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new ApiErrorResponse("토큰이 필요합니다.", "VALIDATION_ERROR"));

                _logger.LogInformation("토큰에서 사용자 정보 추출 요청");

                var user = await _authService.GetUserFromTokenAsync(request.Token);

                if (user == null)
                    return NotFound(new ApiErrorResponse("사용자를 찾을 수 없습니다.", "USER_NOT_FOUND"));

                return Ok(new ApiResponse<UserProfile>
                {
                    Message = "사용자 정보 조회가 완료되었습니다.",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰에서 사용자 정보 추출 중 오류 발생");
                return StatusCode(500, new ApiErrorResponse(
                    "사용자 정보 추출 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        /// <summary>
        /// 사용자 비밀번호를 변경합니다.
        /// </summary>
        /// <param name="request">비밀번호 변경 요청</param>
        /// <returns>변경 결과</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var validationResult = ValidateChangePasswordRequest(request);
                if (validationResult != null)
                    return validationResult;

                _logger.LogInformation("비밀번호 변경 요청: UserId={UserId}", request.UserId);

                var success = await _authService.ChangePasswordAsync(
                    request.UserId,
                    request.CurrentPassword,
                    request.NewPassword);

                if (success)
                {
                    _logger.LogInformation("비밀번호 변경 성공: UserId={UserId}", request.UserId);
                    return Ok(new ApiResponse<bool>
                    {
                        Message = "비밀번호가 성공적으로 변경되었습니다.",
                        Data = true
                    });
                }
                else
                {
                    _logger.LogWarning("비밀번호 변경 실패: UserId={UserId}", request.UserId);
                    return BadRequest(new ApiErrorResponse(
                        "현재 비밀번호가 올바르지 않습니다.",
                        "INVALID_PASSWORD"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "비밀번호 변경 중 오류 발생: UserId={UserId}", request?.UserId);
                return StatusCode(500, new ApiErrorResponse(
                    "비밀번호 변경 중 오류가 발생했습니다.",
                    "INTERNAL_ERROR",
                    ex.Message));
            }
        }

        private IActionResult ValidateChangePasswordRequest(ChangePasswordRequest request)
        {
            if (request == null)
                return BadRequest(new ApiErrorResponse("요청 데이터가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new ApiErrorResponse("사용자 ID가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                return BadRequest(new ApiErrorResponse("현재 비밀번호가 필요합니다.", "VALIDATION_ERROR"));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new ApiErrorResponse("새 비밀번호가 필요합니다.", "VALIDATION_ERROR"));

            if (request.NewPassword.Length < 8)
                return BadRequest(new ApiErrorResponse("새 비밀번호는 8자 이상이어야 합니다.", "VALIDATION_ERROR"));

            if (request.CurrentPassword == request.NewPassword)
                return BadRequest(new ApiErrorResponse("새 비밀번호는 현재 비밀번호와 달라야 합니다.", "VALIDATION_ERROR"));

            return null; // 검증 통과
        }
    }
}
