namespace MailProvider.Models
{
    public class UserAccount
    {
        public string Id { get; set; }
        public string FirstName { get; set;  }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool Active { get; set; }
    }
}
