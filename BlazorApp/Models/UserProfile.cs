namespace BlazorApp.Models
{
    public class UserProfile
    {
        public string Token { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public bool IsUser { get; set; }
        public bool IsAdmin { get; set; }
    }
}
