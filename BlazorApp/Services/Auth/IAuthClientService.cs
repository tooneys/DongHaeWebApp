using BlazorApp.Models;

namespace BlazorApp.Services.Auth
{
    public interface IAuthClientService
    {
        Task<FormResult> LoginAsync(string userId, string password);
        Task LogoutAsync();
        Task<UserProfile?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> ValidateTokenAsync(string token);
    }
}
