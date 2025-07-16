namespace WebApi.Models
{

    public class AuthRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class UserProfile
    {
        public string Token { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public bool IsUser { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class TokenValidationRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
