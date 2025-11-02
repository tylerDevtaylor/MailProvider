namespace MailProvider.Models
{
    public class User
    {
        public string UserId { get; set; }
        public TokenResponse Token { get; set; }

    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
    }
}
