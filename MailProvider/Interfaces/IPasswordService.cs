namespace MailProvider.Interfaces
{
    public interface IPasswordService
    {
        public string HashPassword(string userId, string password);
        public bool ValidateHashedPassword(string userId, string hashedPassword, string password);
    }
}
