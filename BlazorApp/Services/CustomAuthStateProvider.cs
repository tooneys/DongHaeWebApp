using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp.Services.Auth;
using BlazorApp.Models;

namespace BlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthClientService _authClientService;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(
            IAuthClientService authClientService,
            ILogger<CustomAuthStateProvider> logger)
        {
            _authClientService = authClientService;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // anonymous user

            try
            {
                // AuthClientService를 통해 인증 상태 확인
                var isAuthenticated = await _authClientService.IsAuthenticatedAsync();

                if (isAuthenticated)
                {
                    var currentUser = await _authClientService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, currentUser.Username ?? string.Empty),
                            new Claim(ClaimTypes.NameIdentifier, currentUser.UserId ?? string.Empty),
                            new Claim(ClaimTypes.Role, currentUser.IsAdmin ? "Admin" : "User")
                        };

                        var identity = new ClaimsIdentity(claims, "jwt");
                        user = new ClaimsPrincipal(identity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "인증 상태 확인 중 오류 발생");
            }

            return new AuthenticationState(user);
        }

        // 로그인 성공 시 호출되는 메서드 (필요시)
        public void NotifyUserAuthentication()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // 🔥 로그인 메서드 추가
        public async Task<FormResult> LoginAsync(string userId, string password)
        {
            try
            {
                var result = await _authClientService.LoginAsync(userId, password);

                if (result.Successed)
                {
                    // 인증 상태 변경 알림
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그인 중 오류 발생");
                throw;
            }
        }

        // 로그아웃 시 호출되는 메서드
        public async Task LogoutAsync()
        {
            try
            {
                await _authClientService.LogoutAsync();
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthStateProvider 로그아웃 중 오류 발생");
            }
        }
    }
}
