using MailProvider.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MailProvider.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PasswordService> _logger;
        private readonly PasswordHasher<string> _passwordHasher;
        public PasswordService(IConfiguration configuration, ILogger<PasswordService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _passwordHasher = new PasswordHasher<string>();
        }

        public string HashPassword(string userId, string password)
        {
            try
            {
                var hashedPassword = _passwordHasher.HashPassword(userId, password);
                return hashedPassword;
            }
            catch (Exception e)
            {
                _logger.LogError("PasswordService <> HashPassword - unable to hash password for : userId: {userId}.", userId);
                return string.Empty;
            }
        }

        public bool ValidateHashedPassword(string userId, string hashedPassword, string password)
        {
            try
            {
                var validated = _passwordHasher.VerifyHashedPassword(userId, hashedPassword, password);
                return validated == PasswordVerificationResult.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
