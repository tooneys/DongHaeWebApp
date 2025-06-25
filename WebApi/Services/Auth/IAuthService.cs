using WebApi.Models;

namespace WebApi.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// 사용자를 인증합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="password">비밀번호</param>
        /// <returns>인증된 사용자 프로필</returns>
        Task<UserProfile?> Authenticate(string userId, string password);

        /// <summary>
        /// JWT 토큰을 생성합니다.
        /// </summary>
        /// <param name="user">사용자 프로필</param>
        /// <returns>JWT 토큰</returns>
        string GenerateJwtToken(UserProfile user);

        /// <summary>
        /// JWT 토큰의 유효성을 검증합니다.
        /// </summary>
        /// <param name="token">JWT 토큰</param>
        /// <returns>토큰 유효성 여부</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// JWT 토큰에서 사용자 정보를 추출합니다.
        /// </summary>
        /// <param name="token">JWT 토큰</param>
        /// <returns>사용자 프로필</returns>
        Task<UserProfile?> GetUserFromTokenAsync(string token);

        /// <summary>
        /// 사용자 ID로 사용자 정보를 조회합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>사용자 프로필</returns>
        Task<UserProfile?> GetUserByIdAsync(string userId);

        /// <summary>
        /// 사용자 비밀번호를 변경합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="currentPassword">현재 비밀번호</param>
        /// <param name="newPassword">새 비밀번호</param>
        /// <returns>변경 성공 여부</returns>
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// 토큰을 갱신합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>갱신 성공 여부</returns>
        Task<bool> RefreshTokenAsync(string userId);
    }
}
