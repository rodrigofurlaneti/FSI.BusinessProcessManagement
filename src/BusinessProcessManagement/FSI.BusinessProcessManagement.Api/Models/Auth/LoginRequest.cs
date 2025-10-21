namespace FSI.BusinessProcessManagement.Api.Models.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // senha em texto; será verificada via BCrypt
    }
}

