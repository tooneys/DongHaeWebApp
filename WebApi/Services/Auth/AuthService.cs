using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Models;
using WebApi.Infrastructure;
using Dapper;

namespace WebApi.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IDbConnectionFactory connectionFactory,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserProfile?> Authenticate(string userId, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("인증 시도 실패: 사용자 ID 또는 비밀번호가 비어있음");
                    return null;
                }

                _logger.LogInformation("사용자 인증 시작: UserId={UserId}", userId);

                var user = await GetUserFromDatabaseAsync(userId, password);

                if (user == null)
                {
                    _logger.LogWarning("사용자 인증 실패: 잘못된 자격 증명, UserId={UserId}", userId);
                    return null;
                }

                user.Token = GenerateJwtToken(user);
                _logger.LogInformation("사용자 인증 성공: UserId={UserId}, Username={Username}", user.UserId, user.Username);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "사용자 인증 중 오류 발생: UserId={UserId}", userId);
                throw;
            }
        }

        private async Task<UserProfile?> GetUserFromDatabaseAsync(string userId, string password)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@EmpCode", userId);
                parameters.Add("@Password", password);

                var user = await connection.QueryFirstOrDefaultAsync<UserProfile>(
                    "AuthenticateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "데이터베이스에서 사용자 조회 중 오류 발생: UserId={UserId}", userId);
                throw;
            }
        }

        public string GenerateJwtToken(UserProfile user)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expiryHours = int.Parse(jwtSettings["ExpiryHours"] ?? "1");

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    throw new InvalidOperationException("JWT Secret key is not configured");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.IsUser ? "User" : "Admin"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(expiryHours),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogInformation("JWT 토큰 생성 완료: UserId={UserId}, ExpiryTime={ExpiryTime}",
                    user.UserId, tokenDescriptor.Expires);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT 토큰 생성 중 오류 발생: UserId={UserId}", user.UserId);
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                await Task.Run(() => tokenHandler.ValidateToken(token, validationParameters, out _));
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("토큰 만료: Token={Token}", token[..Math.Min(token.Length, 20)] + "...");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "토큰 검증 실패: Token={Token}", token[..Math.Min(token.Length, 20)] + "...");
                return false;
            }
        }

        public async Task<UserProfile?> GetUserFromTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰에서 사용자 정보 추출 중 오류 발생");
                return null;
            }
        }

        public async Task<UserProfile?> GetUserByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var user = await connection.QueryFirstOrDefaultAsync<UserProfile>(
                    "GetUserById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "사용자 조회 중 오류 발생: UserId={UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(currentPassword) ||
                    string.IsNullOrWhiteSpace(newPassword))
                {
                    return false;
                }

                _logger.LogInformation("비밀번호 변경 시작: UserId={UserId}", userId);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CurrentPassword", currentPassword);
                parameters.Add("@NewPassword", newPassword);
                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "ChangeUserPassword",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var result = parameters.Get<int>("@Result");
                var success = result == 1;

                if (success)
                {
                    _logger.LogInformation("비밀번호 변경 성공: UserId={UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("비밀번호 변경 실패: UserId={UserId}", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "비밀번호 변경 중 오류 발생: UserId={UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RefreshTokenAsync(string userId)
        {
            try
            {
                _logger.LogInformation("토큰 갱신 시작: UserId={UserId}", userId);

                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@LastLoginTime", DateTime.UtcNow);

                var affectedRows = await connection.ExecuteAsync(
                    "UpdateUserLastLogin",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 갱신 중 오류 발생: UserId={UserId}", userId);
                throw;
            }
        }
    }
}
